using System;
using System.Net.Sockets;
using Android;
using Android.Content.Res;
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
using Plugin.Clipboard;
using System.Text;

namespace PlexSync
{
    // Default View - Send Magnets to Server
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        const string DIRECTORIES_REQUEST = "__getdirectories__";
        string defaultHostname, defaultPort, hostname, port;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            defaultHostname = GetString(Resource.String.default_hostname);
            defaultPort = GetString(Resource.String.default_port);

            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);


            // First time initialisation
            var prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            if (prefs.GetBoolean(key: "firststart", defValue: true))
            {
                ShowStartDialog(prefs);
            }
            hostname = prefs.GetString(key: "hostname", defValue: defaultHostname);
            port = prefs.GetString(key: "port", defValue: defaultPort);
            
            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);

            var spinner = FindViewById<Spinner>(Resource.Id.spinner);

            initialsieSpinner();

            spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelected);

           

            var clipbrdSender = FindViewById<EditText>(Resource.Id.magnetText);
            clipbrdSender.Click += this.DumpClipboardString;
        }

        async private void initialsieSpinner()
        {
            var spinner = FindViewById<Spinner>(Resource.Id.spinner);
            string[] dirs = new string[] { };

            // sync to server and get the directories

            try
            {
                using var client = new TcpClient
                {
                    SendTimeout = 1000,
                    ReceiveTimeout = 1000
                };
                client.Connect(hostname, 54000);

                var ns = client.GetStream();

                byte[] data = Encoding.UTF8.GetBytes(DIRECTORIES_REQUEST);
                ns.Write(data, 0, data.Length);

                data = new byte[1024];

                int bytes = await ns.ReadAsync(data, 0, data.Length);

                string rawresp = Encoding.UTF8.GetString(data, 0, bytes);

                if (rawresp == string.Empty)
                    throw new FormatException("null response from server when requesting plex directories");

                dirs = rawresp.Split('?');
            }
            catch (Exception ex)
            {
                Snackbar.Make(FindViewById<View>(Resource.Id.textView1), ex.Message, Snackbar.LengthIndefinite)
                       .SetAction("Action", (View.IOnClickListener)null).Show();
            }
            finally
            {

                var ad = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, dirs);


                ad.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                spinner.Adapter = ad;
            }


        }

        private void spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            // do something interesting with this function?
            // change background or smth

            //var spinner = (Spinner)sender;

            //string selected = spinner.SelectedItem.ToString();

            //Toast.MakeText(this, selected, ToastLength.Long).Show();
        }

        private void ShowStartDialog(ISharedPreferences prefs)
        {
            LayoutInflater layoutInflater = LayoutInflater.From(this);
            View view = layoutInflater.Inflate(Resource.Layout.hostname_input_box, null);
            Android.Support.V7.App.AlertDialog.Builder alertBuilder = new Android.Support.V7.App.AlertDialog.Builder(this);
            alertBuilder.SetView(view);

            var hostTextbox = view.FindViewById<EditText>(Resource.Id.hostnameText);
            var portTextbox = view.FindViewById<EditText>(Resource.Id.portText);

            alertBuilder.SetCancelable(true)
                .SetPositiveButton("Submit", delegate
            {
                hostname = hostTextbox.Text;
                port = portTextbox.Text;
            }).SetNegativeButton("Cancel", delegate
            {
                hostname = defaultHostname;
                port = defaultPort;
            });


            Android.Support.V7.App.AlertDialog dialog = alertBuilder.Create();
            dialog.Show();


            var editor = prefs.Edit();
            // add the hostname into the preferences
            editor.PutString("hostname", hostname);
            editor.PutString("port", port);
            // add a boolean tag
            editor.PutBoolean("firststart", false);
            editor.Apply();
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
            if (id == Resource.Id.action_configure)
            {
                ShowStartDialog(PreferenceManager.GetDefaultSharedPreferences(this));
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            // set up the http send and replace the string below with the response

            // send through the spinner id as first byte

            int spinId = FindViewById<Spinner>(Resource.Id.spinner).SelectedItemPosition;
            string text = FindViewById<EditText>(Resource.Id.magnetText).Text;

            if (text == "") return;

            string uri = spinId.ToString() + text;
            string response = string.Empty;

            try
            {
                using (TcpClient client = new TcpClient())
                {
                    client.SendTimeout = 1000;
                    client.ReceiveTimeout = 1000;
                    client.Connect(hostname, int.Parse(port));

                    var ns = client.GetStream();

                    byte[] data = Encoding.UTF8.GetBytes(uri);

                    ns.Write(data, 0, data.Length);

                    data = new byte[1024];

                    // Read the first batch of the TcpServer response bytes.
                    Int32 bytes = ns.Read(data, 0, data.Length);
                    response = Encoding.UTF8.GetString(data, 0, bytes);

                    ns.Close();
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
            }



            View view = (View) sender;
            Snackbar.Make(view, response, Snackbar.LengthLong)
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

