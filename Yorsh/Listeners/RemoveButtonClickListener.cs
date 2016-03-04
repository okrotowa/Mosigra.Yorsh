using Android.Views;
using Yorsh.Data;

namespace Yorsh.Listeners
{
    public class RemoveButtonClickListener : Java.Lang.Object, View.IOnClickListener
    {
        private readonly int _position;

        public RemoveButtonClickListener(int position)
        {
            _position = position;
        }
        public void OnClick(View v)
        {
            Rep.Instance.Players.RemoveAt(_position);
        }
    }
}