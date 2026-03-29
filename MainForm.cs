using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace InventoryManagerDb
{
    public class MainForm : Form
    {
        // Controls
        private TextBox txtName, txtCategory;
        private NumericUpDown numQuantity, numPrice;
        private Button btnAdd, btnUpdate, btnDelete, btnRefresh, btnExit;
        private DataGridView dataGridView1;
        private Label lblStatus;

        public MainForm()
        {
            InitializeUI();
            Db.Initialize();
            LoadProducts();
        }

        private void InitializeUI()
        {
            this.Text = "Inventory Manager";
            this.Width = 800;
            this.Height = 500;

            // Inputs
            txtName = new TextBox { Left = 10, Top = 10, Width = 150 };
            txtCategory = new TextBox { Left = 170, Top = 10, Width = 150 };

            numQuantity = new NumericUpDown { Left = 10, Top = 40, Width = 100, Minimum = 0, Maximum = 10000 };
            numPrice = new NumericUpDown { Left = 120, Top = 40, Width = 100, DecimalPlaces = 2, Minimum = 0, Maximum = 100000 };

            // Buttons
            btnAdd = new Button { Left = 10, Top = 80, Width = 80, Text = "Add" };
            btnUpdate = new Button { Left = 100, Top = 80, Width = 80, Text = "Update" };
            btnDelete = new Button { Left = 190, Top = 80, Width = 80, Text = "Delete" };
            btnRefresh = new Button { Left = 280, Top = 80, Width = 80, Text = "Refresh" };
            btnExit = new Button { Left = 370, Top = 80, Width = 80, Text = "Exit" };

            // Grid
            dataGridView1 = new DataGridView
            {
                Left = 10,
                Top = 120,
                Width = 760,
                Height = 280,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            // Status label
            lblStatus = new Label { Left = 10, Top = 410, Width = 500, Text = "Status: Ready" };

            // Add controls
            Controls.Add(txtName);
            Controls.Add(txtCategory);
            Controls.Add(numQuantity);
            Controls.Add(numPrice);
            Controls.Add(btnAdd);
            Controls.Add(btnUpdate);
            Controls.Add(btnDelete);
            Controls.Add(btnRefresh);
            Controls.Add(btnExit);
            Controls.Add(dataGridView1);
            Controls.Add(lblStatus);

            // Events
            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;
            btnRefresh.Click += (s, e) => LoadProducts();
            btnExit.Click += (s, e) => Application.Exit();
            dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
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
                ClearInputs();
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

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null) return;

            txtName.Text = dataGridView1.CurrentRow.Cells["Name"].Value.ToString();
            txtCategory.Text = dataGridView1.CurrentRow.Cells["Category"].Value.ToString();
            numQuantity.Value = Convert.ToDecimal(dataGridView1.CurrentRow.Cells["Quantity"].Value);
            numPrice.Value = Convert.ToDecimal(dataGridView1.CurrentRow.Cells["Price"].Value);
        }

        private void ClearInputs()
        {
            txtName.Clear();
            txtCategory.Clear();
            numQuantity.Value = 0;
            numPrice.Value = 0;
        }
    }
}