using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace InventoryManagerDb
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Db.Initialize();
            LoadProducts();
        }

        private void LoadProducts()
        {
            try
            {
                using var conn = Db.GetConnection();
                conn.Open();

                string sql = "SELECT * FROM Products";
                using var adapter = new SQLiteDataAdapter(sql, conn);

                var table = new DataTable();
                adapter.Fill(table);

                dataGridView1.DataSource = table;
                lblStatus.Text = $"{table.Rows.Count} products loaded.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) ||
                string.IsNullOrWhiteSpace(txtCategory.Text))
            {
                MessageBox.Show("All fields are required.");
                return false;
            }

            if (numQuantity.Value < 0 || numPrice.Value < 0)
            {
                MessageBox.Show("Values must be >= 0.");
                return false;
            }

            return true;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                using var conn = Db.GetConnection();
                conn.Open();

                string sql = @"INSERT INTO Products 
                              (Name, Category, Quantity, Price)
                              VALUES (@n, @c, @q, @p)";

                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@n", txtName.Text);
                cmd.Parameters.AddWithValue("@c", txtCategory.Text);
                cmd.Parameters.AddWithValue("@q", (int)numQuantity.Value);
                cmd.Parameters.AddWithValue("@p", (double)numPrice.Value);

                cmd.ExecuteNonQuery();

                lblStatus.Text = "Product added.";
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null) return;

            int id = Convert.ToInt32(dataGridView1.CurrentRow.Cells["Id"].Value);

            try
            {
                using var conn = Db.GetConnection();
                conn.Open();

                string sql = @"UPDATE Products 
                               SET Name=@n, Category=@c, Quantity=@q, Price=@p 
                               WHERE Id=@id";

                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@n", txtName.Text);
                cmd.Parameters.AddWithValue("@c", txtCategory.Text);
                cmd.Parameters.AddWithValue("@q", (int)numQuantity.Value);
                cmd.Parameters.AddWithValue("@p", (double)numPrice.Value);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();

                lblStatus.Text = "Product updated.";
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null) return;

            int id = Convert.ToInt32(dataGridView1.CurrentRow.Cells["Id"].Value);

            var confirm = MessageBox.Show("Delete this item?", "Confirm", MessageBoxButtons.YesNo);
            if (confirm != DialogResult.Yes) return;

            try
            {
                using var conn = Db.GetConnection();
                conn.Open();

                string sql = "DELETE FROM Products WHERE Id=@id";

                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();

                lblStatus.Text = "Product deleted.";
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadProducts();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}