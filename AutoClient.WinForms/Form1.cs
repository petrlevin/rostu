using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoClient.Scripts;
using Platform.Web.Services;

namespace AutoClient
{
    public partial class Form1 : Form
    {
        private Redirector redirector;
        
        public Form1()
        {
            InitializeComponent();
            redirector = new Redirector("http://Baranov-PC/Platform3.Trunk/");
        }

        private void btnLoginTest_Click(object sender, EventArgs e)
        {
            redirector.DoRequest<ProfileService>(service => service.Login("admin", "qwe"));
            redirector.DoRequest<ProfileService>(service => service.SetSysDimensions(new Dictionary<string, int>()
                {
                    {"Budget", 1},
                    {"PublicLegalFormation", 1},
                    {"Version", 1}
                }));

            //redirector.DoRequest<DataService>(service => service.GetEntities());
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            new Modules(redirector).AddRange();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            new Modules(redirector).DeleteRange();
        }
    }
}
