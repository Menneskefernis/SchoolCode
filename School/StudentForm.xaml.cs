using School.Data;
using System;
using System.Windows;

namespace School
{
    /// <summary>
    /// Interaction logic for StudentForm.xaml
    /// </summary>
    public partial class StudentForm : Window
    {
        private Student _student;
        public Student StudentEntity
        {
            get
            {
                if (_student == null)
                {
                    _student = new Student();
                }
                _student.FirstName = firstName.Text;
                _student.LastName = lastName.Text;
                _student.DateOfBirth = DateTime.Parse(dateOfBirth.Text);
                return _student;
            }
            set
            {
                _student = value;
                firstName.Text = _student.FirstName;
                lastName.Text = _student.LastName;
                dateOfBirth.Text = _student.DateOfBirth.ToString("d");

                if (OnStudentEntitySet != null)
                {
                    OnStudentEntitySet(_student, new EventArgs());
                }
            }
        }

        public event OnStudentEntitySetHandler OnStudentEntitySet;

        #region Predefined code

        public StudentForm()
        {
            InitializeComponent();
        }

        #endregion

        public delegate void OnStudentEntitySetHandler(Student student, 
            EventArgs e);

        // If the user clicks OK to save the Student details, validate the information that the user has provided
        private void ok_Click(object sender, RoutedEventArgs e)
        {
            if( String.IsNullOrEmpty( firstName.Text.Trim() ) ) {
                MessageBox.Show( "Student must have First Name!", "Error First Name", MessageBoxButton.OK, MessageBoxImage.Error );
                firstName.Focus();
                return;
            }

            if (String.IsNullOrEmpty(lastName.Text.Trim()))
            {
                MessageBox.Show("Student must have Last Name!", "Error Last Name", MessageBoxButton.OK, MessageBoxImage.Error);
                lastName.Focus();
                return;
            }

            DateTime db = DateTime.MinValue;

            if (String.IsNullOrEmpty(dateOfBirth.Text.Trim()) 
                || !DateTime.TryParse(dateOfBirth.Text.Trim(), out db))
            {
                MessageBox.Show("Date of Birth must be valid date!", "Error Date of Birth", MessageBoxButton.OK, MessageBoxImage.Error);
                dateOfBirth.Focus();
                dateOfBirth.SelectAll();
                return;
            }

            
            TimeSpan difference = DateTime.Now.Subtract(db);
            if (5 > difference.Days / 365.25)
            {
                MessageBox.Show("The student must be at least 5 years old!", "Error to young", MessageBoxButton.OK, MessageBoxImage.Error);
                dateOfBirth.Focus();
                dateOfBirth.SelectAll();
                return;
            }

            // Indicate that the data is valid
            this.DialogResult = true;
        }
    }
}
