using System.Linq;
using Yorsh.Helpers;

namespace Yorsh.Model
{
    public static class PurchaseCreator
    {
        public static ErshPurchase Create(string purchaseId)
        {
            var splitProductId = purchaseId.Split('_');
            if (splitProductId.Count() != 2 && splitProductId.Count() != 3) return null;
            if (splitProductId[1] != StringConst.Task && splitProductId[1] != StringConst.Bonus) return null;

            int count;
            var isTask = splitProductId[1] == StringConst.Task;
            if (!int.TryParse(splitProductId[0], out count))
            {
                if (splitProductId[0] != StringConst.All) return null;
                if (isTask) return new AllTaskPurchase(purchaseId);
                return new AllBonusPurchase(purchaseId);
            }
            if (isTask) return new TaskPurchase(purchaseId, count);
            return new BonusPurchase(purchaseId, count);
        }
    }
    public class AllTaskPurchase : ErshPurchase
    {
        public AllTaskPurchase(string purchaseId): base(purchaseId)
        {
            OrdiginalId = purchaseId;
        }

        public override bool IsAll
        {
            get { return true; }
        }

        public override string ProductType
        {
            get { return StringConst.Task; }
        }
    }

    public class TaskPurchase : ErshPurchase
    {
        public TaskPurchase(string purchaseId, int count): base(purchaseId)
        {
            Count = count;
            OrdiginalId = count + '_' + ProductType;
        }

        public override bool IsAll
        {
            get { return false; }
        }

        public override sealed string ProductType
        {
            get { return StringConst.Task; }
        }
    }
    public class BonusPurchase : ErshPurchase
    {
        public BonusPurchase(string purchaseId, int count): base(purchaseId)
        {
            Count = count;
            OrdiginalId = count + '_' + ProductType;
        }

        public override bool IsAll
        {
            get { return false; }
        }

        public override sealed string ProductType
        {
            get { return StringConst.Bonus; }
        }
    }
    public class AllBonusPurchase : ErshPurchase
    {
        public AllBonusPurchase(string purchaseId)
            : base(purchaseId)
        {
        }

        public override bool IsAll
        {
            get { return true; }
        }

        public override string ProductType
        {
            get { return StringConst.Bonus; }
        }
    }

    public abstract class ErshPurchase
    {
        protected ErshPurchase(string purchaseId)
        {
            PurchaseId = purchaseId;
        }
        public string PurchaseId { get; private set; }

        public string OrdiginalId { get; protected set; }
        public int Count { get; protected set; }

        public abstract bool IsAll { get; }

        public abstract string ProductType { get; }

    }
}