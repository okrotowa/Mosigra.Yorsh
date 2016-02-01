using System;
using System.Threading.Tasks;
using Xamarin.Contacts;
using Yorsh.Helpers;
using Android.Graphics;
using Android.App;

namespace Yorsh.Model
{
    public class Player
    {
        private readonly PlayerModel _playerModel;
        public Player(PlayerModel player)
        {
            _playerModel = player;
        }

        public Player()
        {
            _playerModel = new PlayerModel();
        }
        public string PhotoPath
        {
            get { return _playerModel.PhotoPath; }
            set { _playerModel.PhotoPath = value; }
        }

        public string Name
        {
            get { return _playerModel.Name; }
            set { _playerModel.Name = value; }
        }

        public bool IsPlay
        {
            get { return _playerModel.IsPlay; }
            set { _playerModel.IsPlay = value; }
        }

        public int Score
        {
            get { return _playerModel.Score; }
            set { _playerModel.Score = value; }
        }

        public bool FromContacts
        {
            get { return _playerModel.FromContacts; }
            set { _playerModel.FromContacts = value; }
        }

        public Bitmap Image { get; private set; }

        private void SetDefaultImage()
        {
            Image = BitmapFactory.DecodeResource(Application.Context.Resources, Resource.Drawable.photo_default);

        }

        private Bitmap GetDefaultImage()
        {
            return BitmapFactory.DecodeResource(Application.Context.Resources, Resource.Drawable.photo_default);
        }

        public void LoadBitmap(int desiredSize)
        {
            try
            {
                if (string.IsNullOrEmpty(PhotoPath))
                {
                    SetDefaultImage();
                }
                else if (FromContacts)
                {
                    var addressBook = new AddressBook(Application.Context) { PreferContactAggregation = false };
                    var contact = addressBook.Load(PhotoPath);
                    var image = contact.GetThumbnail();
                    Image = image == null ? GetDefaultImage() : image.GetRoundedCornerBitmap();
                }
                else
                {
                    var image = BitmapExtensions.DecodeBitmap(PhotoPath, desiredSize, desiredSize);
                    Image = image == null ? GetDefaultImage() : image.GetRoundedCornerBitmap();
                }
            }
            catch (Exception ex)
            {
                GaService.TrackAppException("Player","LoadBitmap", ex, false);
                SetDefaultImage();
                if (!string.IsNullOrEmpty(PhotoPath)) PhotoPath = null;
            }

        }

        public async Task LoadBitmapAsync(int desiredSize)
        {
            Task.Factory.StartNew(() => LoadBitmap(desiredSize));
        }


        internal PlayerModel GetModel()
        {
            return _playerModel;
        }
    }
}