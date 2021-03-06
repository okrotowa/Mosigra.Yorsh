using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Text;
using Android.Widget;
using Xamarin.Contacts;
using Xamarin.Media;
using Yorsh.Data;
using Yorsh.Fragments;
using Yorsh.Helpers;
using Yorsh.Model;

namespace Yorsh.Activities
{
    [Activity(Label = "@string/AddNewPlayerLowCaseString", ParentActivity = typeof(AddPlayersActivity), ScreenOrientation = ScreenOrientation.Portrait)]
    public class AddNewPlayerActivity : BaseActivity
    {
        Bitmap _playerImage;
        Button _confirmButton;
        Button _chooseFromContactsButton;
        Button _cancelButton;
        EditText _editText;
        ImageButton _playerImageButton;
        private string _playerImagePath;
        private Player _player;

        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                base.OnCreate(bundle);
                SetContentView(Resource.Layout.AddNewPlayer);
                Initialize();
                RegisterSubscribes();
                
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "OnCreate", exception, false);
            }
        }

        protected override void RegisterSubscribes()
        {
            _playerImageButton.Click += ChooseNewPhoto;
            _chooseFromContactsButton.Click += ChooseFromContactsButtonOnClick;
            _confirmButton.Click += ConfirmButtonOnClick;
            _editText.TextChanged += EditTextOnTextChanged;
            _cancelButton.Click += CancelButtonOnClick;

            this.AddButtonTouchListener(_confirmButton);
            this.AddButtonTouchListener(_cancelButton);
            this.AddButtonTouchListener(_chooseFromContactsButton);
        }

        protected override void UnregisterSubscribes()
        {
            _playerImageButton.Click -= ChooseNewPhoto;
            _chooseFromContactsButton.Click -= ChooseFromContactsButtonOnClick;
            _confirmButton.Click -= ConfirmButtonOnClick;
            _editText.TextChanged -= EditTextOnTextChanged;
            _cancelButton.Click -= CancelButtonOnClick;

            var dialog = FragmentManager.FindFragmentByTag("dialog") as ChoosePhotoDialog;
            if (dialog == null) return;
            dialog.ChoosePhoto -= ChoosePhoto_Click;
            dialog.MakePhoto -= MakePhoto_Click;
        }
        
        private void CancelButtonOnClick(object sender, EventArgs eventArgs)
        {
            this.StartActivityWithoutBackStack(new Intent(this, typeof(AddPlayersActivity)));
        }

        private void EditTextOnTextChanged(object sender, TextChangedEventArgs e)
        {
            var text = e.Text.ToString();
            SetConfirmButtonEnabled(!string.IsNullOrEmpty(text));
        }

        private void ConfirmButtonOnClick(object sender, EventArgs eventArgs)
        {
            var name = FindViewById<EditText>(Resource.Id.playerName).Text;
            _player.Name = name;
            Rep.Instance.Players.Add(_player);
            this.StartActivityWithoutBackStack(new Intent(this, typeof(AddPlayersActivity)));
        }

        private void ChooseFromContactsButtonOnClick(object sender, EventArgs eventArgs)
        {
            var contactPickerIntent = new Intent(Intent.ActionPick, ContactsContract.Contacts.ContentUri);
            StartActivityForResult(contactPickerIntent, 3);
        }

        private void Initialize()
        {
            try
            {
                _chooseFromContactsButton = FindViewById<Button>(Resource.Id.chooseFromContButton);
                _confirmButton = FindViewById<Button>(Resource.Id.confirmButton);
                _cancelButton = FindViewById<Button>(Resource.Id.cancelButton);
                _editText = FindViewById<EditText>(Resource.Id.playerName);
                _playerImageButton = FindViewById<ImageButton>(Resource.Id.playerImage);

                SetFont(_cancelButton);
                SetFont(_confirmButton);
                SetFont(_chooseFromContactsButton);
                SetFontItalic(_editText);



                _confirmButton.Background.Alpha = 255;
                _confirmButton.SetTextColor(Resources.GetColor(Resource.Color.white));
                _cancelButton.Background.Alpha = 255;
                _cancelButton.SetTextColor(Resources.GetColor(Resource.Color.white));

                _confirmButton.Enabled = true; SetConfirmButtonEnabled(false);


                _player = new Player();
                //To set _playImage to default
                PlayerImagePath = null;
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "Initialize", exception, false);
            }
        }


        private string PlayerImagePath
        {
            get
            {
                return _player.PhotoPath;
            }
            set
            {
                RunOnUiThread(() =>
                {
                    try
                    {
                        _player.PhotoPath = value;
                        _player.LoadBitmap(this.Resources.GetDimensionPixelSize(
                            Resource.Dimension.AddPlayerItem_imageSize));
                        _playerImageButton.SetImageBitmap(_player.Image);

                    }
                    catch (Exception exception)
                    {
                        GaService.TrackAppException(this.Class, "PlayerImagePath", exception, false);
                        _playerImageButton.SetImageResource(Resource.Drawable.photo_default);
                    }
                });

            }
        }

        private void ChoosePhoto_Click(object sender, EventArgs e)
        {
            try
            {
                var picker = new MediaPicker(this);
                if (!picker.PhotosSupported)
                {
                    ShowUnsupported();
                    return;
                }

                var intent = picker.GetPickPhotoUI();
                this.StartActivityForResult(intent, 2);
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "ChoosePhoto_Click", exception, false);
            }

        }

        private void MakePhoto_Click(object sender, EventArgs e)
        {
            try
            {
                var picker = new MediaPicker(this);
                if (!picker.IsCameraAvailable || !picker.PhotosSupported)
                {
                    ShowUnsupported();
                    return;
                }
                var store = new StoreCameraMediaOptions
                {
                    Directory = "Ersh",
                    Name = string.Format("ersh_{0}.jpg", DateTime.Now.Ticks + new Random().Next())
                };

                var intent = picker.GetTakePhotoUI(store);
                this.StartActivityForResult(intent, 2);
            }

            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "MakePhoto_Click", exception, false);
            }

        }

        private void ShowUnsupported()
        {
            var unsupportedToast = Toast.MakeText(this, Resources.GetString(Resource.String.NotSupportCameraString), ToastLength.Short);
            unsupportedToast.Show();
        }

        private void ChooseNewPhoto(object sender, EventArgs e)
        {
            try
            {
                var prev = FragmentManager.FindFragmentByTag("dialog");

                ChoosePhotoDialog dialog;
                if (prev == null)
                {
                    dialog = new ChoosePhotoDialog();
                    dialog.ChoosePhoto += ChoosePhoto_Click;
                    dialog.MakePhoto += MakePhoto_Click;
                }
                else dialog = (ChoosePhotoDialog)prev;

                var fragmentTransaction = FragmentManager.BeginTransaction();
                dialog.Show(fragmentTransaction, "dialog");
            }

            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "ChooseNewPhoto", exception, false);
            }
        }

        //Get Photo and Name from Contact
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                if (resultCode == Result.Canceled)
                    return;

                if (requestCode == 2)
                {
                    _player.FromContacts = false;
                    data.GetMediaFileExtraAsync(this).ContinueWith(t =>
                        PlayerImagePath = t.Result.Path);
                }

                if (requestCode == 3)
                {
                    if (data == null || data.Data == null)
                        return;

                    var addressBook = new AddressBook(Application.Context)
                    {
                        PreferContactAggregation = false
                    };

                    var contact = addressBook.Load(data.Data.LastPathSegment);

                    if (string.IsNullOrEmpty(contact.DisplayName))
                        Toast.MakeText(this, Resources.GetString(Resource.String.NoNameString), ToastLength.Short)
                            .Show();
                    else
                        _editText.Text = contact.DisplayName;


                    if (contact.GetThumbnail() != null)
                    {
                        _player.FromContacts = true;
                        PlayerImagePath = data.Data.LastPathSegment;
                    }
                    else
                    {
                        _player.FromContacts = false;
                        PlayerImagePath = null;
                    }
                }
            }

            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "OnActivityResult", exception, false);
            }
        }

        private void SetConfirmButtonEnabled(bool enabled)
        {
            try
            {
                if (_confirmButton.Enabled == enabled) return;
                _confirmButton.Enabled = enabled;
                _confirmButton.SetTextColor(enabled ? Resources.GetColor(Resource.Color.white) : this.GetColorWithOpacity(Resource.Color.white, Resource.Color.button_text_disabled));
                if (enabled)
                    _confirmButton.Background.ClearColorFilter();
                else
                    _confirmButton.Background.SetColorFilter(Resources.GetColor(Resource.Color.button_disabled), PorterDuff.Mode.SrcAtop);
            }

            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "SetConfirmButtonEnabled", exception, false);
            }

        }

        private void SetFont(TextView textView)
        {
            textView.SetTypeface(Rep.FontManager.Get(Font.BoldCondensed), TypefaceStyle.Normal);
        }

        private void SetFontItalic(TextView textView)
        {
            textView.SetTypeface(Rep.FontManager.Get(Font.Condensed), TypefaceStyle.Italic);
        }

        protected override void OnDestroy()
        {
            if (_playerImageButton.Drawable != null)
            {
                _playerImageButton.Drawable.Dispose();
                _playerImageButton.SetImageBitmap(null);
            }
            //_confirmButton.Background.ClearColorFilter();
            //_confirmButton.SetTextColor(Resources.GetColor(Resource.Color.white));
            base.OnDestroy();
        }
    }
}