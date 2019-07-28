using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

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
    [Activity(Label = "View Downloads", Theme = "@style/AppTheme.NoActionBar")]
    public class ViewDownloads : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        private const string downloadsRequest = "__listtorrents__";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_downloads);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);

             



            RequestTorrents();
        }

        private void RequestTorrents()
        {
            List<string> torrents = new List<string>();
            const int port = 54000;
            try
            {
                using (var client = new TcpClient())
                {
                    client.SendTimeout = 1000;
                    client.ReceiveTimeout = 1000;


                    client.Connect("192.168.0.2", port);

                    var ns = client.GetStream();

                    byte[] data = Encoding.UTF8.GetBytes(downloadsRequest);
                    ns.Write(data, 0, data.Length);

                    data = new byte[1024];

                    Int32 bytes = ns.Read(data, 0, data.Length);

                    torrents = ParseServerResponse(data, bytes);

                    ns.Close();
                    client.Close();
                }
            }
            catch (System.IO.IOException)
            {
                Snackbar.Make(FindViewById<View>(Resource.Id.tablelayout), $"Port {port} is busy", Snackbar.LengthIndefinite)
                       .SetAction("Action", (View.IOnClickListener)null).Show();
                return;
            }
            catch(SocketException)
            {
                Snackbar.Make(FindViewById<View>(Resource.Id.tablelayout), "No response from host", Snackbar.LengthIndefinite)
                    .SetAction("Action", (View.IOnClickListener)null).Show();
                return;
            }
            catch(TimeoutException ex)
            {
                Snackbar.Make(FindViewById<View>(Resource.Id.tablelayout), ex.Message, Snackbar.LengthIndefinite)
                       .SetAction("Action", (View.IOnClickListener)null).Show();
                return;
            }
            finally
            {
                TableLayout table = FindViewById<TableLayout>(Resource.Id.tablelayout);
                var layout = new TableRow.LayoutParams(
                    ViewGroup.LayoutParams.MatchParent,
                    ViewGroup.LayoutParams.MatchParent);
                
                foreach (string s in torrents)
                {
                    // fucks up everything?
                    string[] split = s.Split('~');

                    TableRow row = new TableRow(this);
                    row.LayoutParameters = layout;

                    TextView text = new TextView(this)
                    {
                        Text = split[0]

                    };
                    text.LayoutParameters = layout;
                    text.SetMaxWidth(165);
                    row.AddView(text, 0);
                    
                    

                    text = new TextView(this)
                    {
                        Text = split[1]

                    };
                    text.LayoutParameters = layout;
                    row.AddView(text, 1);

                    text = new TextView(this)
                    {
                        Text = split[2]

                    };
                    text.LayoutParameters = layout;
                    row.AddView(text, 2);


                    table.AddView(row, layout);
                }
            }
        }

        private List<string> ParseServerResponse(byte[] data, Int32 bytes)
        {
            string rawresp = Encoding.UTF8.GetString(data, 0, bytes);
            if (rawresp == "")
                return new List<string>() { "" };

            // split on the seperator ','
            List<string> dirs = rawresp.Split('\n').ToList();

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
            else if (id == Resource.Id.nav_folder)
            {
                StartActivity(new Intent(this, typeof(ViewFolder)));

            }
            else if(id == Resource.Id.nav_manage)
            {
                StartActivity(new Intent(this, typeof(Tools)));
            }

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }
    }
}