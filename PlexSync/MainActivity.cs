using System;
using System.Net.Sockets;
using Android;
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
using Plugin.Clipboard;

namespace PlexSync
{
    // Default View - Send Magnets to Server
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);

            var clipbrdSender = FindViewById<EditText>(Resource.Id.magnetText);
            clipbrdSender.Click += this.DumpClipboardString;
        }

        private async void DumpClipboardString(object sender, EventArgs e)
        {
            // empty the contents of the clipboard into magnetText

            var magnettxt = (EditText)sender;

            string clipboardText = await CrossClipboard.Current.GetTextAsync();

            magnettxt.Text = clipboardText;

//#if DEBUG
//            magnettxt.Text = "magnet:?xt=urn:btih:4bf843445b1a19a42dd884f20a5931e42d107b35&dn=South.Park.S22E01.Dead.Kids.720p.WEBRip.AAC2.0.H.264.mkv&tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969&tr=udp%3A%2F%2Ftracker.openbittorrent.com%3A80&tr=udp%3A%2F%2Fopen.demonii.com%3A1337&tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969&tr=udp%3A%2F%2Fexodus.desync.com%3A6969";
//#endif
        }

        public override void OnBackPressed()
        {
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            if(drawer.IsDrawerOpen(GravityCompat.Start))
            {
                drawer.CloseDrawer(GravityCompat.Start);
            }
            else
            {
                base.OnBackPressed();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            // set up the http send and replace the string below with the response


            string uri = FindViewById<EditText>(Resource.Id.magnetText).Text;

            if (uri == "") return;
            string response = string.Empty;
            const int port = 54000;

            try
            {
                using (TcpClient client = new TcpClient())
                {
                    client.SendTimeout = 1000;
                    client.ReceiveTimeout = 1000;
                    client.Connect("192.168.0.2", port);

                    var ns = client.GetStream();

                    byte[] data = System.Text.Encoding.UTF8.GetBytes(uri);

                    ns.Write(data, 0, data.Length);

                    data = new byte[1024];

                    // Read the first batch of the TcpServer response bytes.
                    Int32 bytes = ns.Read(data, 0, data.Length);
                    response = System.Text.Encoding.UTF8.GetString(data, 0, bytes);

                    ns.Close();
                    client.Close();
                }
            }
            catch (System.IO.IOException ex)
            {
                response = ex.Message;
            }
            catch (TimeoutException ex)
            {
                response = ex.Message;
            }
            catch (ArgumentNullException ex)
            {
                response = ex.Message;
            }
            catch (SocketException ex)
            {
                response = ex.Message;
            }
            catch (Exception ex)
            {
                response = ex.Message;
            }



            View view = (View) sender;
            Snackbar.Make(view, response, Snackbar.LengthIndefinite)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            int id = item.ItemId;

            if (id == Resource.Id.nav_folder)
            {
                StartActivity(new Intent(this, typeof(ViewFolder)));
            
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

