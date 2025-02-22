using Newtonsoft.Json.Linq;
using System.Data;
using System.Windows.Forms;

namespace JSON_Files
{
    public partial class Form1 : Form
    {
        private JArray jArrayData;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JSON Files|*.json";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string json = System.IO.File.ReadAllText(ofd.FileName);
                jArrayData = JArray.Parse(json);
                dataGridView1.DataSource = ConvertToDataTable(jArrayData);
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (jArrayData == null || jArrayData.Count == 0) return;

            string filterText = txtSearch.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(filterText))
            {
                dataGridView1.DataSource = ConvertToDataTable(jArrayData);
                return;
            }

            var filteredData = jArrayData
                .Where(item => item.Children<JProperty>()
                    .Any(prop => prop.Value.ToString().ToLower().Contains(filterText)))
                .ToList();

            dataGridView1.DataSource = ConvertToDataTable(new JArray(filteredData));
        }

        private DataTable ConvertToDataTable(JArray jArray)
        {
            DataTable dt = new DataTable();
            if (jArray.Count == 0) return dt;

            foreach (JProperty column in jArray[0].Children<JProperty>())
            {
                if (!dt.Columns.Contains(column.Name))
                    dt.Columns.Add(column.Name);
            }

            foreach (JObject obj in jArray)
            {
                DataRow row = dt.NewRow();
                foreach (JProperty column in obj.Properties())
                {
                    row[column.Name] = column.Value.ToString();
                }
                dt.Rows.Add(row);
            }

            return dt;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (dataGridView1.DataSource == null)
            {
                MessageBox.Show("There is no data to save.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "JSON Files|*.json";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                JArray updatedJArray = ConvertToJArray((DataTable)dataGridView1.DataSource);

                System.IO.File.WriteAllText(sfd.FileName, updatedJArray.ToString());

                MessageBox.Show("Data saved successfully.", "success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private JArray ConvertToJArray(DataTable dt)
        {
            JArray jArray = new JArray();

            foreach (DataRow row in dt.Rows)
            {
                JObject obj = new JObject();
                foreach (DataColumn column in dt.Columns)
                {
                    obj[column.ColumnName] = row[column].ToString();
                }
                jArray.Add(obj);
            }

            return jArray;
        }
    }
}
