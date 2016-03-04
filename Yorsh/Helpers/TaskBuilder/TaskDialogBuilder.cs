using System;
using Android.App;
using Yorsh.Model;

namespace Yorsh.Helpers
{
    public static class TaskDialogBuilder
    {
        public static TaskDialogAbstract GetTask(TaskDialog taskDialog, int taskScore, PlayerModel player)
        {
            switch (taskDialog)
            {
                case TaskDialog.Bear:
                    return new BearTask(player.Name, player.Score);
                case TaskDialog.Make:
                    return new MakeTask(taskScore,player.Name,player.Score);
                case TaskDialog.RefuseFirstTime:
                    return new RefuseTask(taskScore);
                case TaskDialog.RefuseSecondTime:
                    return new RefuseAndMoveTask(player.Name, player.Score);
                default: throw new NotImplementedException();
            }
        }
    }

    public abstract  class TaskDialogAbstract
    {
		readonly string[] _taskStrings;

		protected TaskDialogAbstract()
		{			
			_taskStrings = Application.Context.Resources.GetStringArray (ResourceArray);
		}

		public abstract int ResourceArray { get; }

        public virtual string GetStatusTitle()
        {
			return _taskStrings[0];
        }

        public virtual string GetStartDesc()
        {
			return _taskStrings[1];
        }
        
        public virtual string GetChangedScore()
        {
			return _taskStrings[2];
        }

        public virtual string GetEndDesc()
        {
			return _taskStrings[3];
        }
        public virtual string GetCurentScore()
        {
			return _taskStrings [4];
        }

    }

}