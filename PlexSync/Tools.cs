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
using System.Net;
using System.Threading.Tasks;
using System.Threading;

namespace PlexSync
{
    [Activity(Label = "Tools", Theme = "@style/AppTheme.NoActionBar")]
    public class Tools : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        private const string timeString = "__gettime__";
        private const string refreshString = "__refreshplex__";
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

            FindViewById<Button>(Resource.Id.button_refresh).Click += this.Tools_Click;

            try
            {
                client = new TcpClient();
                client.SendTimeout = 1000;
                client.ReceiveTimeout = 1000;

                client.Connect("192.168.0.2", 54000);




            }
            catch (System.IO.IOException ex)
            {
                Snackbar.Make(FindViewById<View>(Resource.Id.tablelayout), ex.Message, Snackbar.LengthIndefinite)
                       .SetAction("Action", (View.IOnClickListener)null).Show();
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
            init();
        }

        private void Tools_Click(object sender, EventArgs e)
        {
            if (!client.Connected)
                return;

            try
            {
                var ns = client.GetStream();
                byte[] data = Encoding.UTF8.GetBytes(refreshString);

                ns.Write(data, 0, data.Length);
            }
            catch (System.IO.IOException)
            {
                init();
            }
        }

        private void init()
        {
            if(client.Connected)
            {
                FindViewById<TextView>(Resource.Id.text_deviceip1).Text = this.client.Client.LocalEndPoint.ToString();

                FindViewById<TextView>(Resource.Id.text_status1).Text = "Connected";
                FindViewById<TextView>(Resource.Id.text_status1).SetTextColor(Android.Graphics.Color.Green);

                FindViewById<TextView>(Resource.Id.text_server1).Text = this.client.Client.RemoteEndPoint.ToString();

                FindViewById<TextView>(Resource.Id.text_trans1).Text = this.client.Available.ToString() + " KB";

                // for uptime- ping the server and do uptime request.
                Thread t = new Thread(ServerTimeLoop);
                t.Start();
            }
            else
            {
                // lets update with what we can do
                FindViewById<TextView>(Resource.Id.text_deviceip1).Text = this.client.Client.LocalEndPoint.ToString();
                FindViewById<TextView>(Resource.Id.text_status1).Text = "Not Connected";
                FindViewById<TextView>(Resource.Id.text_status1).SetTextColor(Android.Graphics.Color.Red);
            }
        }

        async private void ServerTimeLoop()
        {
            DateTime start = new DateTime();
            var textview = FindViewById<TextView>(Resource.Id.text_uptime1);
            string response = string.Empty;

            try
            {
                var ns = client.GetStream();
                byte[] data = Encoding.UTF8.GetBytes(timeString);

                ns.Write(data, 0, data.Length);

                data = new byte[1024];

                int bytes = await ns.ReadAsync(data, 0, data.Length);

                response = Encoding.UTF8.GetString(data, 0, bytes);

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
                       .SetAction("Action", (View.IOnClickListener)null).Show();
                return;
            }
            finally
            {
                start = DateTime.ParseExact(response, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);


            }
           
               
          

            while (client.Connected)
            {
                DateTime now = DateTime.Now;
                TimeSpan delta = start - now;
                textview.Text = $"{delta.ToString("hh")}:{delta.ToString("mm")}:{delta.ToString("ss")}";
            }
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
                client.Close();
                StartActivity(new Intent(this, typeof(MainActivity)));

            }
            else if (id == Resource.Id.nav_folder)
            {
                client.Close();
                StartActivity(new Intent(this, typeof(ViewFolder)));

            }
            else if (id == Resource.Id.nav_downloads)
            {
                client.Close();
                StartActivity(new Intent(this, typeof(ViewDownloads)));
            }

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }

        public override void Finish()
        {
            base.Finish();

            client.Close();
        }
    }
    
    

}