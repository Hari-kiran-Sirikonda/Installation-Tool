
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;

namespace MachineEnvironmentManager
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new EnvironmentManagerForm());
        }
    }

    public class EnvironmentManagerForm : Form
    {
        private TextBox txtName;
        private TextBox txtValue;
        private DataGridView grid;
        private Button btnAdd, btnUpdate, btnDelete, btnRefresh;

        // Protect important Windows environment variables
        private readonly string[] protectedVariables = {
            "Path",
            "SystemRoot",
            "windir",
            "ComSpec",
            "PATHEXT",
            "OS"
        };

        public EnvironmentManagerForm()
        {
            InitializeUI();
            LoadVariables();
        }

        private void InitializeUI()
        {
            Text = "Machine Environment Variable Manager";
            Size = new Size(900, 900);
            StartPosition = FormStartPosition.CenterScreen;

            Label heading = new Label
            {
                Text = "Machine Environment Variables",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(25, 20),
                AutoSize = true
            };

            Label lblName = new Label
            {
                Text = "Variable Name",
                Location = new Point(30, 85),
                AutoSize = true
            };

            txtName = new TextBox
            {
                Location = new Point(30, 110),
                Width = 350
            };

            Label lblValue = new Label
            {
                Text = "Variable Value",
                Location = new Point(420, 85),
                AutoSize = true
            };

            txtValue = new TextBox
            {
                Location = new Point(420, 110),
                Width = 430
            };

            btnAdd = new Button
            {
                Text = "Add",
                Location = new Point(30, 155),
                Size = new Size(100, 35)
            };

            btnUpdate = new Button
            {
                Text = "Update",
                Location = new Point(145, 155),
                Size = new Size(100, 35)
            };

            btnDelete = new Button
            {
                Text = "Delete",
                Location = new Point(260, 155),
                Size = new Size(100, 35)
            };

            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(375, 155),
                Size = new Size(100, 35)
            };

            grid = new DataGridView
            {
                Location = new Point(30, 215),
                Size = new Size(820, 310),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false
            };

            grid.Columns.Add("Name", "Variable Name");
            grid.Columns.Add("Value", "Variable Value");

            grid.CellClick += Grid_CellClick;

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += (s, e) => LoadVariables();

            Controls.AddRange(new Control[]
            {
                heading, lblName, txtName,
                lblValue, txtValue,
                btnAdd, btnUpdate, btnDelete,
                btnRefresh, grid
            });
        }

        // CHECK WHETHER VARIABLE IS PROTECTED
        private bool IsProtectedVariable(string name)
        {
            foreach (string variable in protectedVariables)
            {
                if (string.Equals(
                    variable,
                    name,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        // READ - LOAD ALL MACHINE ENVIRONMENT VARIABLES
        private void LoadVariables()
        {
            grid.Rows.Clear();

            try
            {
                var variables =
                    Environment.GetEnvironmentVariables(
                        EnvironmentVariableTarget.Machine
                    );

                foreach (DictionaryEntry item in variables)
                {
                    grid.Rows.Add(
                        item.Key.ToString(),
                        item.Value?.ToString()
                    );
                }

                grid.Sort(
                    grid.Columns["Name"],
                    System.ComponentModel.ListSortDirection.Ascending
                );
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        // READ - SELECT INDIVIDUAL VARIABLE
        private void Grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                string name = grid.Rows[e.RowIndex]
                    .Cells["Name"].Value?.ToString();

                txtName.Text = name;

                txtValue.Text =
                    Environment.GetEnvironmentVariable(
                        name,
                        EnvironmentVariableTarget.Machine
                    ) ?? "";
            }
        }

        // CREATE - ADD NEW MACHINE ENVIRONMENT VARIABLE
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string value = txtValue.Text;

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Enter a variable name.");
                return;
            }

            if (name.Contains('='))
            {
                MessageBox.Show(
                    "Variable name cannot contain '='."
                );
                return;
            }

            try
            {
                string existingValue =
                    Environment.GetEnvironmentVariable(
                        name,
                        EnvironmentVariableTarget.Machine
                    );

                if (existingValue != null)
                {
                    MessageBox.Show(
                        "Variable already exists. Use Update instead."
                    );
                    return;
                }

                Environment.SetEnvironmentVariable(
                    name,
                    value,
                    EnvironmentVariableTarget.Machine
                );

                MessageBox.Show(
                    "Variable added successfully!"
                );

                ClearFields();
                LoadVariables();
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        // UPDATE - MODIFY EXISTING MACHINE ENVIRONMENT VARIABLE
        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string value = txtValue.Text;

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Select a variable first.");
                return;
            }

            try
            {
                string existingValue =
                    Environment.GetEnvironmentVariable(
                        name,
                        EnvironmentVariableTarget.Machine
                    );

                if (existingValue == null)
                {
                    MessageBox.Show(
                        "Variable does not exist."
                    );
                    return;
                }

                if (IsProtectedVariable(name))
                {
                    MessageBox.Show(
                        "This is a protected Windows variable " +
                        "and cannot be modified through this application."
                    );
                    return;
                }

                Environment.SetEnvironmentVariable(
                    name,
                    value,
                    EnvironmentVariableTarget.Machine
                );

                MessageBox.Show(
                    "Variable updated successfully!"
                );

                ClearFields();
                LoadVariables();
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        // DELETE - REMOVE MACHINE ENVIRONMENT VARIABLE
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show(
                    "Select a variable to delete."
                );
                return;
            }

            if (IsProtectedVariable(name))
            {
                MessageBox.Show(
                    "This is a protected Windows variable " +
                    "and cannot be deleted through this application."
                );
                return;
            }

            DialogResult result = MessageBox.Show(
                $"Are you sure you want to delete '{name}'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                try
                {
                    string existingValue =
                        Environment.GetEnvironmentVariable(
                            name,
                            EnvironmentVariableTarget.Machine
                        );

                    if (existingValue == null)
                    {
                        MessageBox.Show(
                            "Variable does not exist."
                        );
                        return;
                    }

                    Environment.SetEnvironmentVariable(
                        name,
                        null,
                        EnvironmentVariableTarget.Machine
                    );

                    MessageBox.Show(
                        "Variable deleted successfully!"
                    );

                    ClearFields();
                    LoadVariables();
                }
                catch (Exception ex)
                {
                    ShowError(ex);
                }
            }
        }

        // CLEAR INPUT FIELDS
        private void ClearFields()
        {
            txtName.Clear();
            txtValue.Clear();
        }

        // DISPLAY ERRORS
        private void ShowError(Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Operation Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}
