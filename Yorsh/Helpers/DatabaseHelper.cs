using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Database.Sqlite;
using SQLite;
using Yorsh.Data;
using Yorsh.Model;
using Yorsh.Model.EventAgruments;

namespace Yorsh.Helpers
{
    public sealed class DatabaseHelper : SQLiteOpenHelper
    {
        private readonly Context _context;
        readonly object _locker = new object();
        private TaskList _tasks;
        private SQLiteAsyncConnection _dataBaseConnection;
        private IList<BonusTable> _bonuses;
        private IList<ErshPurchase> _taskPurchases = new List<ErshPurchase>();
        private IList<ErshPurchase> _bonusPurchases = new List<ErshPurchase>();

        internal DatabaseHelper(Context context, int version) : base(context, Rep.Instance.DataBaseFile, null, version)
        {
            _context = context;           
        }

        public event EventHandler DataBaseCreatedOrOpened;

        private void OnDataBaseCreatedOrOpened()
        {
            EventHandler handler = DataBaseCreatedOrOpened;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler DatabaseChanged;
        private void OnDatabaseChanged()
        {
            var handler = DatabaseChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        //Refresh and check if 
        public async Task CheckAndRefreshPurshases(IList<ErshPurchase> ershPurchases, bool isConnected)
        {
            try
            {
                _taskPurchases = ershPurchases.Where(purchase => purchase.ProductType == StringConst.Task).ToList();
                _bonusPurchases = ershPurchases.Where(purchase => purchase.ProductType == StringConst.Bonus).ToList();
                if (!isConnected) return;

                var currentPurchaseTaskCount = Tasks.Count - IntConst.DefaultTaskCount;
                var purchaseTaskCount = GetPurchaseCount(_taskPurchases, IntConst.AllTaskCount, IntConst.DefaultTaskCount);
                await CorrectingPurchaseAsync(currentPurchaseTaskCount, purchaseTaskCount, AddProductAsync<TaskTable>, RemoveTableAsync<TaskTable>);

                var currentPurchaseBonusCount = Bonuses.Count - IntConst.DefaultBonusCount;
                var purchaseBonusCount = GetPurchaseCount(_bonusPurchases, IntConst.AllBonusCount, IntConst.DefaultBonusCount);
                await CorrectingPurchaseAsync(currentPurchaseBonusCount, purchaseBonusCount, AddProductAsync<BonusTable>, RemoveTableAsync<BonusTable>);
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "CheckAndRefreshPurshases", exception, false);
            }

        }
        private async Task CorrectingPurchaseAsync(int currentPurchase, int realPurchase, Func<int, Task> addFunc, Func<int, Task> removeFunc)
        {
            try
            {
                if (currentPurchase == realPurchase) return;
                var difference = Math.Abs(currentPurchase - realPurchase);

                if (currentPurchase > realPurchase)
                    await removeFunc(difference);
                else if (currentPurchase < realPurchase)
                    await addFunc(difference);
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "CorrectingPurchaseAsync", exception, false);
            }

        }

        private int GetPurchaseCount(IList<ErshPurchase> purchases, int allCount, int defaultCount)
        {
            var count = 0;
            try
            {
                if (purchases == null || !purchases.Any()) return 0;
                foreach (var purchase in purchases)
                {
                    if (purchase.IsAll) return allCount - defaultCount;
                    count += purchase.Count;
                }
                return count;
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "GetPurchaseCount", exception, false);
                return count;
            }

        }

        public TaskList Tasks
        {
            get { return _tasks; }
            private set
            {
                 if (_tasks != null && _tasks.Enumerator != null) _tasks.Enumerator.TaskPositionChanged -= EnumeratorOnTaskPositionChanged;
                _tasks = value;
                _tasks.Enumerator.TaskPositionChanged += EnumeratorOnTaskPositionChanged;
				//ImageActivityHelper = new ImageActivityHelper(Tasks.GetTask, Tasks.GetCategory, _context.Resources.DisplayMetrics);
                OnDatabaseChanged();
            }
        }
        public IList<BonusTable> Bonuses
        {
            get
            {
                return _bonuses;
            }
            private set
            {
                _bonuses = value;
                OnDatabaseChanged();
            }
        }
        public async Task AddProductAsync(ErshPurchase purchase)
        {
            try
            {
                switch (purchase.ProductType)
                {
                    case StringConst.Task:
                        AddProductAsync<TaskTable>(purchase.IsAll ? IntConst.AllTaskCount - Tasks.Count : purchase.Count);
                        break;
                    case StringConst.Bonus:
                        AddProductAsync<BonusTable>(purchase.IsAll ? IntConst.AllBonusCount - Bonuses.Count : purchase.Count);
                        break;
                }
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "AddProductAsync", exception, true);
            }
        }

        private async Task AddProductAsync<T>(int count) where T : class, ITable, new()
        {
            try
            {
                var list = await DataBaseConnection.Table<T>().Where(p => p.IsEnabled == false).Take(count).ToListAsync();
                foreach (var table in list)
                {
                    table.IsEnabled = true;
                }
                await DataBaseConnection.UpdateAllAsync(list);
                await RefreshTableAsync<T>();
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, string.Format("AddProductAsync<{0}>", new T()), exception, true);
            }
        }

        private async Task RemoveTableAsync<T>(int count) where T : class, ITable, new()
        {
            try
            {
                var list = await DataBaseConnection.Table<T>().Where(p => p.IsEnabled).Take(count).ToListAsync();
                foreach (var table in list)
                {
                    table.IsEnabled = false;
                    var taskTable = table as TaskTable;
                    if (taskTable != null) taskTable.Position = 0;
                }
                await DataBaseConnection.UpdateAllAsync(list);
                await RefreshTableAsync<T>();
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "RemoveBonusAsync", exception, false);
            }

        }

        private IEnumerable<T> GetTable<T>(Func<string, T> creator, Func<int, bool> isEnabled, Stream stream) where T : ITable, new()
        {
            var list = new List<T>();
            try
            {
                using (stream)
                {
                    using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                    {
                        var index = 0;
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            if (line == null) continue;
                            var product = creator(line);
                            product.IsEnabled = isEnabled(index);
                            list.Add(product);
                            index++;
                        }

                    }
                }
                return list;
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, string.Format("GetTable<{0}>", new T().TableName), exception, false);
                return list;
            }
        }

        private IEnumerable<T> GetTable<T>() where T : ITable, new()
        {
            try
            {
                switch (new T().TableName)
                {
                    case StringConst.BonusTable:
                        return (IEnumerable<T>)GetTable(Parser.ParseBonus, i => i < IntConst.DefaultBonusCount, _context.Assets.Open("Bonus.csv"));
                    case StringConst.TaskTable:
                        return (IEnumerable<T>)GetTable(Parser.ParseTask, i => i < IntConst.DefaultTaskCount, _context.Assets.Open("Task.csv"));
                    case StringConst.CategoryTable:
                        return (IEnumerable<T>)GetTable(Parser.ParseCategory, i => true, _context.Assets.Open("Category.csv"));
                    default: throw new NotImplementedException();
                }
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, string.Format("GetTable<{0}>", new T().TableName), exception, false);
                throw;
            }

        }
        private async Task CreateIfNotExistTable<T>() where T : ITable, new()
        {
            try
            {
                await DataBaseConnection.CreateTableAsync<T>();
                var count = await DataBaseConnection.Table<T>().CountAsync();
                if (count == 0)
                {
                    var elements = GetTable<T>();
                    await DataBaseConnection.InsertAllAsync(elements);
                }
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, string.Format("CreateIfNotExistTable<{0}>", new T()), exception, false);
            }

        }

        public async Task CreateOrOpenDataBaseAsync()
        {
            try
            {
                await CreateIfNotExistTable<BonusTable>();
                await CreateIfNotExistTable<TaskTable>();
                await CreateIfNotExistTable<CategoryTable>();
                if (await DataBaseConnection.Table<CategoryTable>().Where(category=>category.Image==null).CountAsync()!=0)
                {
                    await RecreateTable<CategoryTable>();
                    await TaskScoreRefreshAsync();
                }

                await RefreshAllAsync();
                OnDataBaseCreatedOrOpened();
                
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(_context.Class.SimpleName, "CreateOrOpenDataBaseAsync", exception, true);
            }
        }

        private async Task RecreateTable<TAble>() where TAble : ITable, new()
        {
            await DataBaseConnection.DropTableAsync<TAble>();
            await CreateIfNotExistTable<TAble>();
        }

        public SQLiteAsyncConnection DataBaseConnection
        {
            get
            {
                lock (_locker)
                {
                    return _dataBaseConnection ?? (_dataBaseConnection = new SQLiteAsyncConnection(Rep.Instance.DataBaseFile));
                }
            }
        }

        private async Task RefreshAllAsync()
        {
            try
            {
                await TaskRefreshAsync();
                Bonuses = await DataBaseConnection.Table<BonusTable>().Where(product => product.IsEnabled).ToListAsync();
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "RefreshAllAsync", exception, false);
            }

        }
        private async Task RefreshTableAsync<T>() where T : ITable, new()
        {
            try
            {
                switch (new T().TableName)
                {
                    case StringConst.CategoryTable:
                        break;
                    case StringConst.BonusTable:
                        Bonuses = await DataBaseConnection.Table<BonusTable>().Where(product => product.IsEnabled).ToListAsync();
                        break;
                    case StringConst.TaskTable:
                        await TaskRefreshAsync(); break;
                    default: throw new NotImplementedException();

                }
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, string.Format("RefreshTableAsync<{0}>", new T().TableName), exception, false);
            }
        }
		private async Task TaskScoreRefreshAsync()
		{
			try
			{
				await DataBaseConnection.CreateTableAsync<TaskTable>();
				var currentTasks = await DataBaseConnection.Table<TaskTable>().ToListAsync();
				if (currentTasks.Count == 0) return;
				var tasks = GetTable<TaskTable>();
				foreach(var task in tasks)
				{
					var currentTask = currentTasks.FirstOrDefault(x=>x.Id == task.Id);
					if (currentTask == null) continue;
					currentTask.Score = task.Score;
				}
				await DataBaseConnection.UpdateAllAsync(currentTasks);
			}
			catch (Exception exception)
			{
				GaService.TrackAppException(this.Class,"TaskScoreRefreshAsync", exception, false);
			}
		}

        private async Task TaskRefreshAsync()
        {
            try
            {
                var categoryList = await DataBaseConnection.Table<CategoryTable>().ToListAsync();
                var taskList = await DataBaseConnection.Table<TaskTable>().Where(product => product.IsEnabled).ToListAsync();
                int currentPosition;
                var sortedTaskList = taskList.CustomSort(out currentPosition);
                Tasks = new TaskList(sortedTaskList, categoryList, currentPosition);
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "TaskRefreshAsync", exception, false);
            }
        }

        private async void EnumeratorOnTaskPositionChanged(object sender, TaskPositionChangedEventArgs e)
        {
            await SaveTaskContextAsync(e.TaskTable);
        }

        public async Task SaveTaskContextAsync(TaskTable taskTable)
        {
            try
            {
                await DataBaseConnection.UpdateAsync(taskTable);
            }
            catch (Exception exception)
            {
                var activityName = string.Format("SaveTaskContextAsync: tableId={0}, talbePosition={1}",
                    taskTable == null ? (object)null : taskTable.Id,
                    taskTable == null ? (object)null : taskTable.Position);
                GaService.TrackAppException(this.Class, activityName, exception, false);
            }
        }
        
		public async Task SaveTaskContextAsync()
        {
            try
            {
                await DataBaseConnection.UpdateAllAsync(Tasks.Tasks);
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "SaveTaskContextAsync", exception, false);
            }
        }
        public async Task ResetAsync()
        {
            try
            {
                Tasks.Clear();
                await SaveTaskContextAsync();
                await TaskRefreshAsync();
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "ResetAsync", exception, false);
            }

        }

        public override void OnCreate(SQLiteDatabase db)
        {
            
        }

        public async override void OnUpgrade(SQLiteDatabase db, int oldVersion, int newVersion)
        {
            try
            {
                await DataBaseConnection.DropTableAsync<TaskTable>();
	            await DataBaseConnection.DropTableAsync<BonusTable>();
	            await DataBaseConnection.DropTableAsync<CategoryTable>();
				await CreateOrOpenDataBaseAsync();
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "OnUpgrade", exception, false);
            }

        }
    }
}