using System;
using System.Data.SqlServerCe;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BGC
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class clsSettings : Window
    {

        public clsSettings() // constructor
        {
            InitializeComponent();
            txtAutoGames.Text = Properties.Settings.Default.CVsCgameNumber.ToString();
            txtMovePause.Text = Properties.Settings.Default.CVsCSecPause.ToString();
            txtShowMoves.Text = Properties.Settings.Default.CVsCshowMoves.ToString();
            txtShowGames.Text = Properties.Settings.Default.CVsCshowGames.ToString();
            txtTemp1.Text = Properties.Settings.Default.UseProgrammedTemp0.ToString();
            txtTemp2.Text = Properties.Settings.Default.UseProgrammedTemp1.ToString();
            txtTemperaments.Text = getTemperaments();
            
        }

        private string getTemperaments()
        {
            try
            {
                // There is a dependecy on SQL Server Compact 4.0 SP1 https://www.microsoft.com/en-au/download/details.aspx?id=30709
                // Installed SQLLite/SQL Server Compact Toolbox
                // Get to the point where it's showing up in the data connections
                string AppPath = System.AppDomain.CurrentDomain.BaseDirectory;
                SqlCeConnection conn = new SqlCeConnection(@"Data Source=" + AppPath + "Database1.sdf");
                conn.Open();
                SqlCeCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Sum(Score) as tR0" +
                    ",  Sum(CASE WHEN TemperID = 1 THEN Score ELSE 0 END) as tR1" +
                    ",  Sum(CASE WHEN TemperID = 2 THEN Score ELSE 0 END) as tR2" +
                    ",  Sum(CASE WHEN TemperID = 3 THEN Score ELSE 0 END) as tR3" +
                    ",  Sum(CASE WHEN TemperID = 4 THEN Score ELSE 0 END) as tR4" +
                    ",  Sum(CASE WHEN TemperID = 5 THEN Score ELSE 0 END) as tR5" +
                    ",  Sum(CASE WHEN TemperID = 6 THEN Score ELSE 0 END) as tR6" +
                    ",  Sum(CASE WHEN TemperID = 7 THEN Score ELSE 0 END) as tR7" +
                    " FROM bgmemories";
                SqlCeDataReader dr = cmd.ExecuteReader();
                String summary = "";
                while (dr.Read())
                {
                    //0 random, 1 running, 2 make points 1, 3 blitz, 4 not blot, 5 adjacent, 6 bear in, 7 norm 
                    summary = "Run:" + dr.GetValue(1).ToString();
                    summary += ", Make points:" + dr.GetValue(2).ToString();
                    summary += ", Blitz:" + dr.GetValue(3).ToString();
                    summary += ", Be safe:" + dr.GetValue(4).ToString();
                    summary += ", Clump:" + dr.GetValue(5).ToString();
                    summary += ", Home:" + dr.GetValue(6).ToString();
                    summary += ", Norm:" + dr.GetValue(7).ToString();
                }
                conn.Close();
                return summary;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return ex.Message;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Dialog box canceled
            this.DialogResult = false;
        }

        void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            // Don't accept the dialog box if there is invalid data
            if (!IsValid(this)) return;

            this.DialogResult = true;
        }

        // Validate all dependency objects in a window
        bool IsValid(DependencyObject node)
        {
            // Check if dependency object was passed
            if (node != null)
            {
                // Check if dependency object is valid.
                // NOTE: Validation.GetHasError works for controls that have validation rules attached
                bool isValid = !Validation.GetHasError(node);
                if (!isValid)
                {
                    // If the dependency object is invalid, and it can receive the focus,
                    // set the focus
                    if (node is IInputElement) Keyboard.Focus((IInputElement)node);
                    return false;
                }
            }

            // If this dependency object is valid, check all child dependency objects
            foreach (object subnode in LogicalTreeHelper.GetChildren(node))
            {
                if (subnode is DependencyObject)
                {
                    // If a child dependency object is invalid, return false immediately,
                    // otherwise keep checking
                    if (IsValid((DependencyObject)subnode) == false) return false;
                }
            }

            // All dependency objects are valid
            return true;
        }
    }

    // This should be in it's own class file.
    public class TemperamentValidationRule : ValidationRule
    {
        double minTemperament;
        double maxTemperament;

        public double MinTemperament
        {
            get { return this.minTemperament; }
            set { this.minTemperament = value; }
        }

        public double MaxTemperament
        {
            get { return this.maxTemperament; }
            set { this.maxTemperament = value; }
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            double temperament;

            // Is a number?
            if (!double.TryParse((string)value, out temperament))
            {
                return new ValidationResult(false, "Not a number.");
            }

            // Is in range?
            if ((temperament < this.minTemperament) || (temperament > this.maxTemperament))
            {
                string msg = string.Format("Temperament must be between {0} and {1}.", this.minTemperament, this.maxTemperament);
                return new ValidationResult(false, msg);
            }

            // Number is valid
            return new ValidationResult(true, null);
        }
    }
}
