using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace course_project.Forms
{
    public partial class SellerMenuForm : Form
    {
        public SellerMenuForm()
        {
            InitializeComponent();
        }

        private void CashierForm_Load(object sender, EventArgs e)
        {

        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonUpdateUser_Click(object sender, EventArgs e)
        {
            this.Close();
            AuthForm authForm = new AuthForm();
            authForm.Show();
        }

        private void buttonSale_Click(object sender, EventArgs e)
        {
            this.Close();
            SellerSaleForm saleForm = new SellerSaleForm();
            saleForm.Show();
        }

        private void buttonProducts_Click(object sender, EventArgs e)
        {
            this.Close();
            ProductsForm productsForm = new ProductsForm();
            productsForm.Show();
        }

        private void buttonReport_Click(object sender, EventArgs e)
        {
            this.Close();
            ReportsForm reportsForm = new ReportsForm();    
            reportsForm.Show();
        }
    }
}
