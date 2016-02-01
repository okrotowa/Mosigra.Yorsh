using System;
using Android.Views;

namespace Yorsh.Activities
{
    interface IBaseActivity
    {
        View CreateActionButton(int resourceId);
        View ActionButton { get; }

        void SetHomeButtonEnabled(bool enabled);

        bool AllowBackPressed { get; set; }

    }
}