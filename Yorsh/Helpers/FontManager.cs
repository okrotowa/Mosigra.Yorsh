using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Graphics;

namespace Yorsh.Helpers
{
    public class FontManager
    {
        private Context _context;
        private IDictionary<Font, Typeface> _fontResources;
        public FontManager(Application app)
        {
            _context = app.ApplicationContext;
            _fontResources = new Dictionary<Font, Typeface>();
            InitResources();
        }

        private void InitResources()
        {
             _fontResources.Add(Font.BoldCondensed, Typeface.CreateFromAsset(_context.Assets, "MyriadProBoldCondensed.ttf"));  
             _fontResources.Add(Font.Condensed, Typeface.CreateFromAsset(_context.Assets, "MyriadProCondensed.ttf"));  
             _fontResources.Add(Font.Bold, Typeface.CreateFromAsset(_context.Assets, "MyriadProBold.ttf"));  

            _fontResources.Add(Font.Regular, Typeface.CreateFromAsset(_context.Assets, "MyriadProRegular.ttf"));
            _fontResources.Add(Font.SemiboldCondensed, Typeface.CreateFromAsset(_context.Assets,  "MyriadProSemiboldCond.otf"));
            _fontResources.Add(Font.LightCondensed, Typeface.CreateFromAsset(_context.Assets, "MyriadProLightCond.otf"));

            _fontResources.Add(Font.BankirRetro, Typeface.CreateFromAsset(_context.Assets, "BankirRetro.ttf"));
        }

        public Typeface Get(Font font)
        {
            return _fontResources[font];
        }

    }
}