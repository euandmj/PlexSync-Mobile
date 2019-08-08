using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace PlexSync
{
    internal struct Torrent
    {
        public string Name;
        public string Progress;
        public string State;

        public void SetProgress(string val)
        {
            if (double.TryParse(val, out double x))
            {
                Progress = Math.Round(x * 100, 3).ToString() + "%";
            }
            else
                Progress = val;
        }
    }

    [Activity(Label = "View Downloads", Theme = "@style/AppTheme.NoActionBar")]
    public class ViewDownloads : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        private Dictionary<string, Torrent> activeDownloads; 
        private SwipeRefreshLayout swipeRefreshLayout;

        private const string downloadsRequest = "__listtorrents__";
        private string hostname;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_downloads);


            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            activeDownloads = new Dictionary<string, Torrent>();

            var prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            hostname = prefs.GetString(key: "hostname", defValue: GetString(Resource.String.default_hostname));


            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);

            swipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.rootLayout);
            swipeRefreshLayout.SetColorSchemeColors(new int[] { Android.Resource.Color.BackgroundLight });
            swipeRefreshLayout.Refresh += this.SwipeRefreshLayout_Refresh;
            

            RequestTorrents();
        }

        private void SwipeRefreshLayout_Refresh(object sender, EventArgs e)
        {
            
            Thread t = new Thread(RequestTorrents);
            t.Start();


            swipeRefreshLayout.Refreshing = false;
        }

        async private void RequestTorrents()
        {
            List<string> torrents = new List<string>();
            const int port = 54000;
            try
            {
                using (var client = new TcpClient())
                {
                    client.SendTimeout = 1000;
                    client.ReceiveTimeout = 1000;


                    client.Connect(hostname, port);

                    var ns = client.GetStream();

                    byte[] data = Encoding.UTF8.GetBytes(downloadsRequest);
                    ns.Write(data, 0, data.Length);

                    data = new byte[1024];

                    
                    int bytes = await ns.ReadAsync(data, 0, data.Length);
                    //int bytes = ns.Read(data, 0, data.Length);

                    torrents = ParseServerResponse(data, bytes);

                    ns.Close();
                    client.Close();
                }
            }
            catch (System.IO.IOException ex)
            {
                Snackbar.Make(FindViewById<View>(Resource.Id.rootLayout), ex.Message, Snackbar.LengthLong)
                       .SetAction("Action", (View.IOnClickListener)null).Show();
                return;
            }
            catch(SocketException ex)
            {
                Snackbar.Make(FindViewById<View>(Resource.Id.rootLayout), ex.Message, Snackbar.LengthLong)
                       .SetAction("Action", (View.IOnClickListener)null).Show();
                return;
            }
            catch(TimeoutException ex)
            {
                Snackbar.Make(FindViewById<View>(Resource.Id.rootLayout), ex.Message, Snackbar.LengthLong)
                       .SetAction("Action", (View.IOnClickListener)null).Show();
                return;
            }
            finally
            {
                try
                {
                    // build the dictionary
                    foreach (string s in torrents)
                    {
                        string[] split = s.Split('~');
                        // 0 - hash (key)
                        // 1 - name
                        // 2 - progress
                        // 3 - state

                        var t = new Torrent()
                        {
                            Name = split[1],
                            State = split[3]
                        };
                        t.SetProgress(split[2]);

                        activeDownloads[split[0]] = t;
                        
                        RunOnUiThread(UpdateListView);
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    throw;
                }
            }
        }

        private void UpdateListView()
        {
            var table = FindViewById<TableLayout>(Resource.Id.tableLayout1);
            var layout = new TableRow.LayoutParams(
                   ViewGroup.LayoutParams.WrapContent,
                   ViewGroup.LayoutParams.WrapContent);

            // clear the table apart from header
            for(int i = 1; i < table.ChildCount; i++)
            {
                table.RemoveViewAt(i);
            }

            int rowcolorcount = 0;
            foreach(var pair in activeDownloads)
            {
                TableRow row = new TableRow(this);

                TextView text = new TextView(this)
                {
                    Text = pair.Value.Name

                };
                text.SetMaxWidth(160);
                text.SetMinWidth(160);
                text.LayoutParameters = layout;
                row.AddView(text, 0);

                text = new TextView(this)
                {                                       
                    Text = pair.Value.Progress
                };
                text.LayoutParameters = layout;
                row.AddView(text, 1);

                text = new TextView(this)
                {
                    Text = pair.Value.State

                };
                text.LayoutParameters = layout;
                row.AddView(text, 2);

                // change colour 
                if (rowcolorcount++ % 2 == 0)
                    row.SetBackgroundColor(Android.Graphics.Color.LightBlue);
                table.AddView(row, layout);
            }
        }

        private List<string> ParseServerResponse(byte[] data, Int32 bytes)
        {
            string rawresp = Encoding.UTF8.GetString(data, 0, bytes);
            if (rawresp == "")
                return new List<string>();

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