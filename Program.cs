
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
            SessionLogger.Initialize();

            try
            {
                Application.Run(new EnvironmentManagerForm());
            }
            finally
            {
                SessionLogger.SaveSession();
            }
        }
    }

    public class EnvironmentManagerForm : Form
    {
        private TextBox txtName;
        private TextBox txtValue;
        private TextBox txtLog;
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
            SessionLogger.MessageLogged += SessionLogger_MessageLogged;
            LoadVariables(false);
        }

        private void InitializeUI()
        {
            Text = "Machine Environment Variable Manager";
            ClientSize = new Size(900, 740);
            StartPosition = FormStartPosition.CenterScreen;

            Label heading = new Label
            {
                Text = "Machine Environment Variables",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label lblName = new Label
            {
                Text = "Variable Name",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            txtName = new TextBox
            {
                Dock = DockStyle.Fill
            };

            Label lblValue = new Label
            {
                Text = "Variable Value",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            txtValue = new TextBox
            {
                Dock = DockStyle.Fill
            };

            btnAdd = new Button
            {
                Text = "Add",
                Size = new Size(100, 35),
                Margin = new Padding(0, 5, 15, 5)
            };
            btnAdd.Click += BtnAdd_Click;


            btnUpdate = new Button
            {
                Text = "Update",
                Size = new Size(100, 35),
                Margin = new Padding(0, 5, 15, 5)
            };
            btnUpdate.Click += BtnUpdate_Click;

            btnDelete = new Button
            {
                Text = "Delete",
                Size = new Size(100, 35),
                Margin = new Padding(0, 5, 15, 5)
            };
            btnDelete.Click += BtnDelete_Click;

            btnRefresh = new Button
            {
                Text = "Refresh",
                Size = new Size(100, 35),
                Margin = new Padding(0, 5, 15, 5)
            };
            btnRefresh.Click += (s, e) => LoadVariables();

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
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

            Label lblLog = new Label
            {
                Text = "Action Log",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            txtLog = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.Black,
                ForeColor = Color.LightGreen,
                Font = new Font("Consolas", 9F)
            };

            TableLayoutPanel inputLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(0, 0, 0, 5)
            };
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            TableLayoutPanel nameLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0, 0, 15, 0)
            };
            nameLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            nameLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            nameLayout.Controls.Add(lblName, 0, 0);
            nameLayout.Controls.Add(txtName, 0, 1);

            TableLayoutPanel valueLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(15, 0, 0, 0)
            };
            valueLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            valueLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            valueLayout.Controls.Add(lblValue, 0, 0);
            valueLayout.Controls.Add(txtValue, 0, 1);

            inputLayout.Controls.Add(nameLayout, 0, 0);
            inputLayout.Controls.Add(valueLayout, 1, 0);

            FlowLayoutPanel buttonLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            buttonLayout.Controls.AddRange(new Control[]
            {
                btnAdd, btnUpdate, btnDelete, btnRefresh
            });

            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                Padding = new Padding(25, 15, 25, 25)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
            mainLayout.Controls.Add(heading, 0, 0);
            mainLayout.Controls.Add(inputLayout, 0, 1);
            mainLayout.Controls.Add(buttonLayout, 0, 2);
            mainLayout.Controls.Add(grid, 0, 3);
            mainLayout.Controls.Add(lblLog, 0, 4);
            mainLayout.Controls.Add(txtLog, 0, 5);

            grid.CellClick += Grid_CellClick;
            Controls.Add(mainLayout);
        }

        private void SessionLogger_MessageLogged(object? sender, string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.BeginInvoke(() => SessionLogger_MessageLogged(sender, message));
                return;
            }

            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
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
        private void LoadVariables(bool logAction = true)
        {
            grid.Rows.Clear();
            if (logAction)
            {
                SessionLogger.Info("Refreshing machine environment variables.");
            }

            try
            {
                var variables =
                    Environment.GetEnvironmentVariables(
                        EnvironmentVariableTarget.Machine
                    );

                foreach (DictionaryEntry item in variables)
                {
                    string key = item.Key.ToString();

                    if(key.StartsWith("EA_", StringComparison.OrdinalIgnoreCase))
                    {
                        grid.Rows.Add(
                            item.Key.ToString(),
                            item.Value?.ToString()
                        );
                if (logAction)
                {
                    SessionLogger.Info($"Loaded {grid.Rows.Count} EA_ environment variable(s).");
                }
                    }
                    
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

                SessionLogger.Info($"Selected variable '{name}'.");
            }
        }

        // CREATE - ADD NEW MACHINE ENVIRONMENT VARIABLE
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string value = txtValue.Text;

            if (string.IsNullOrWhiteSpace(name))
            {
                SessionLogger.Info("Add skipped: enter a variable name.");
                return;
            }

            if (name.Contains('='))
            {
                SessionLogger.Info("Add skipped: variable name cannot contain '='.");
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
                    SessionLogger.Info($"Add skipped: variable '{name}' already exists.");
                    return;
                }

                Environment.SetEnvironmentVariable(
                    name,
                    value,
                    EnvironmentVariableTarget.Machine
                );

                SessionLogger.Info($"Variable '{name}' added successfully.");

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
                SessionLogger.Info("Update skipped: select a variable first.");
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
                    SessionLogger.Info($"Update skipped: variable '{name}' does not exist.");
                    return;
                }

                if (IsProtectedVariable(name))
                {
                    SessionLogger.Info($"Update skipped: variable '{name}' is protected.");
                    return;
                }

                Environment.SetEnvironmentVariable(
                    name,
                    value,
                    EnvironmentVariableTarget.Machine
                );

                SessionLogger.Info($"Variable '{name}' updated successfully.");

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
                SessionLogger.Info("Delete skipped: select a variable first.");
                return;
            }

            if (IsProtectedVariable(name))
            {
                SessionLogger.Info($"Delete skipped: variable '{name}' is protected.");
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
                        SessionLogger.Info($"Delete skipped: variable '{name}' does not exist.");
                        return;
                    }

                    Environment.SetEnvironmentVariable(
                        name,
                        null,
                        EnvironmentVariableTarget.Machine
                    );

                    SessionLogger.Info($"Variable '{name}' deleted successfully.");

                    ClearFields();
                    LoadVariables();
                }
                catch (Exception ex)
                {
                    ShowError(ex);
                }
            }
            else
            {
                SessionLogger.Info($"Delete canceled for variable '{name}'.");
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
            SessionLogger.Error("Operation failed", ex);
        }
    }
}
