using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace PlexSync
{
    [Activity(Label = "Tools", Theme = "@style/AppTheme.NoActionBar")]
    public class Tools : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        private TcpClient client;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_tools);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);

            try
            {
                client = new TcpClient();
                client.SendTimeout = 1000;
                client.ReceiveTimeout = 1000;

                client.Connect("192.168.0.2", 54000);
            }
            catch (SocketException)
            {
                Snackbar.Make(FindViewById<View>(Resource.Id.tablelayout), "No response from host", Snackbar.LengthIndefinite)
                    .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
            }
            catch (TimeoutException ex)
            {
                Snackbar.Make(FindViewById<View>(Resource.Id.tablelayout), ex.Message, Snackbar.LengthIndefinite)
                       .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
            }
        }

        private void init()
        {


        }

        public override void OnBackPressed()
        {
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            if (drawer.IsDrawerOpen(GravityCompat.Start))
            {
                drawer.CloseDrawer(GravityCompat.Start);
            }
            else
            {
                base.OnBackPressed();
            }
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            int id = item.ItemId;

            if (id == Resource.Id.nav_magnet)
            {
                StartActivity(new Intent(this, typeof(MainActivity)));

            }
            else if (id == Resource.Id.nav_folder)
            {
                StartActivity(new Intent(this, typeof(ViewFolder)));

            }
            else if (id == Resource.Id.nav_downloads)
            {
                StartActivity(new Intent(this, typeof(ViewDownloads)));
            }

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }
    }
}