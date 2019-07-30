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
using System.Threading;

namespace PlexSync
{
    [Activity(Label = "View Folder", Theme = "@style/AppTheme.NoActionBar")]
    public class ViewFolder : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        private List<string> directories; 
        private const string folderRequest = "__listdownloaded__";
        private SwipeRefreshLayout swipeRefreshLayout;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_folder);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            directories = new List<string>();

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);

            swipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.rootLayout);
            swipeRefreshLayout.SetColorSchemeColors(new int[] { Android.Resource.Color.BackgroundLight });
            swipeRefreshLayout.Refresh += this.SwipeRefreshLayout_Refresh;

            RequestDirectories();
        }

        private void SwipeRefreshLayout_Refresh(object sender, EventArgs e)
        {
            Thread t = new Thread(RequestDirectories);
            t.Start();

            swipeRefreshLayout.Refreshing = false;
        }

        async private void RequestDirectories()
        {
            // connect to server and request directories
            //FindViewById<TextView>(Resource.Id.errorTextView).Text = string.Empty;
            
            const int port = 54000;

            try
            {
                using (var client = new TcpClient())
                {
                    client.SendTimeout = 1000;
                    client.ReceiveTimeout = 1000;
                    client.Connect("192.168.0.2", port);

                    var ns = client.GetStream();

                    byte[] data = Encoding.UTF8.GetBytes(folderRequest);
                    ns.Write(data, 0, data.Length);

                    data = new byte[1024];

                    int bytes = await ns.ReadAsync(data, 0, data.Length);

                    directories = ParseServerResponse(data, bytes);

                    ns.Close();
                    client.Close();
                }
            }
            catch (System.IO.IOException ex)
            {
                Snackbar.Make(FindViewById<View>(Resource.Id.rootLayout), ex.Message, Snackbar.LengthIndefinite)
                       .SetAction("Action", (View.IOnClickListener)null).Show();
                return;
            }
            catch (SocketException ex)
            {
                Snackbar.Make(FindViewById<View>(Resource.Id.rootLayout), ex.Message, Snackbar.LengthIndefinite)
                       .SetAction("Action", (View.IOnClickListener)null).Show();
                return;
            }
            catch (TimeoutException ex)
            {
                Snackbar.Make(FindViewById<View>(Resource.Id.rootLayout), ex.Message, Snackbar.LengthIndefinite)
                       .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
                return;
            }
            finally
            {
                RunOnUiThread(UpdateTableView);
            }            
        }

        private void UpdateTableView()
        {
            // add the strings into the table layout
            TableLayout table = FindViewById<TableLayout>(Resource.Id.tableLayout);
            var layout = new TableRow.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent);
            table.RemoveAllViews();

            int i = 0;
            foreach (string s in directories)
            {
                TableRow row = new TableRow(this);

                TextView text = new TextView(this)
                {
                    Text = s
                };

                text.TextSize = 15;


                row.AddView(text);

                if (i++ % 2 == 0)
                    row.SetBackgroundColor(Android.Graphics.Color.LightBlue);
                table.AddView(row);

            }
        }

        private List<string> ParseServerResponse(byte[] data, Int32 bytes)
        {
            string rawresp = Encoding.UTF8.GetString(data, 0, bytes);
            if (rawresp == "")
                return new List<string>();

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
                StartActivity(new Intent(this, typeof(MainActivity)));

            }
            else if (id == Resource.Id.nav_downloads)
            {
                StartActivity(new Intent(this, typeof(ViewDownloads)));
            }
            else if (id == Resource.Id.nav_manage)
            {
                StartActivity(new Intent(this, typeof(Tools)));
            }


            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }
    }
}