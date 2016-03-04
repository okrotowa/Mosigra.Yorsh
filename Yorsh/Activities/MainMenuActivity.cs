using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Yorsh.Data;
using Yorsh.Helpers;
using Android.Content.PM;
using Android.Text;
using System;
using Android.Graphics;

namespace Yorsh.Activities
{
    [Activity(Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainMenuActivity : HelpActivity
    {
		TextView _boardGameButton;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.MainMenu);
            SetLinkView();
            var myriadProBoldCons = Rep.FontManager.Get(Font.BoldCondensed);

            SetButton(Resource.Id.StartGame, typeof(AddPlayersActivity), myriadProBoldCons);
            SetButton(Resource.Id.Rules, typeof(RulesActivity), myriadProBoldCons);
            SetButton(Resource.Id.PlusCards, typeof(StoreActivity), myriadProBoldCons);
            this.SaveAsStartupActivity(StringConst.MainMenuActivity);

            RegisterSubscribes();
        }

        protected override void RegisterSubscribes()
        {
			_boardGameButton.Click += _boardGameButton_Click;
        }

        void _boardGameButton_Click (object sender, EventArgs e)
        {
			GetAlertDialog().Show();
        }

        protected override void UnregisterSubscribes()
        {
			_boardGameButton.Click -= _boardGameButton_Click;
        }

        private void SetButton(int resorceId, Type activityOnClick, Typeface font)
        {
            var startGameButton = FindViewById<Button>(resorceId);
            AddButtonTouchListener(startGameButton);
            //startGameButton.Touch += (sender, e) => this.OnTouchButtonDarker(startGameButton, e);
            startGameButton.Click += (sender, e) => this.StartActivityWithoutBackStack(new Intent(this, activityOnClick));
            startGameButton.SetTypeface(font, TypefaceStyle.Normal);
        }

        private void SetLinkView()
        {
            _boardGameButton = FindViewById<TextView>(Resource.Id.BoardGame);
			_boardGameButton.SetTypeface(Rep.FontManager.Get(Font.BoldCondensed),
                TypefaceStyle.Normal);
            var webLinkText = @"<a href='http://www.spb.mosigra.ru/Face/Show/ersh'>"
                + GetString(Resource.String.BoardGameString)
                + "</a>";
            var textFormated = Html.FromHtml(webLinkText);
			_boardGameButton.TextFormatted = textFormated;//your html goes in responseText
			_boardGameButton.Clickable = true;
        }

        private AlertDialog.Builder GetAlertDialog()
        {
            var builder = new AlertDialog.Builder(this);
            builder.SetMessage(Resource.String.OpenSiteQuestionString);
            builder.SetTitle(Resource.String.GoToMosigraSite);
            builder.SetPositiveButton(GetString(Resource.String.YesString), delegate
            {
                var uri = Android.Net.Uri.Parse("http://www.spb.mosigra.ru/Face/Show/ersh");
                var intent = new Intent(Intent.ActionView, uri);
                StartActivity(intent);
            });
            builder.SetNegativeButton(GetString(Resource.String.NoString), delegate
            {
            });
            return builder;
        }

    }
}

