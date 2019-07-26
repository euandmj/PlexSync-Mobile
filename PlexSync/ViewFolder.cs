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
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar")]
    public class ViewFolder : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        private const string folderRequest = "__listdownloaded__";


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_folder);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);

            RequestDirectories();
        }

        private void RequestDirectories()
        {
            // connect to server and request directories
            //FindViewById<TextView>(Resource.Id.errorTextView).Text = string.Empty;
            List<string> dirs = new List<string>();

            try
            {
                using (var client = new TcpClient())
                {

                    client.Connect("192.168.0.2", 54000);
                    client.SendTimeout = 1500;
                    client.ReceiveTimeout = 1500;

                    var ns = client.GetStream();

                    byte[] data = Encoding.ASCII.GetBytes(folderRequest);
                    ns.Write(data, 0, data.Length);

                    data = new byte[1024];

                    Int32 bytes = ns.Read(data, 0, data.Length);

                    dirs = ParseServerResponse(data, bytes);

                    ns.Close();
                    client.Close();
                }
            }
            catch(SocketException ex)
            {
                FindViewById<TextView>(Resource.Id.errorTextView).Text = ex.Message;
            }

            // add the strings into the table layout
            TableLayout table = FindViewById<TableLayout>(Resource.Id.tablelayout);
            var layout = new TableRow.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent);


            foreach (string s in dirs)
            {
                TableRow row = new TableRow(this);

                TextView text = new TextView(this)
                {
                    Text = s
                };

                
                row.AddView(text, 0);
                table.AddView(row);
            }
        }

        private List<string> ParseServerResponse(byte[] data, Int32 bytes)
        {
            string rawresp = Encoding.ASCII.GetString(data, 0, bytes);
            if (rawresp == "")
                return new List<string>() { "" };

            // split on the seperator ','
            List<string> dirs = rawresp.Split(',').ToList();

            // remove any whitespace
            dirs.ForEach(s => s.Trim());

            return dirs;
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
                Intent next = new Intent(this, typeof(MainActivity));
                StartActivity(next);

            }

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }
    }
}