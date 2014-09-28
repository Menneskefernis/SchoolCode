using System;

using System.Collections;
using System.ComponentModel;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using System.Data;
using System.Data.Objects;

using School.Data;


namespace School
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Connection to the School database
        private SchoolDBEntities schoolContext = null;

        // Field for tracking the currently selected teacher
        private Teacher teacher = null;

        // List for tracking the students assigned to the teacher's class
        private IList studentsInfo = null;

        #region Predefined code

        public MainWindow()
        {
            InitializeComponent();
        }

        // Connect to the database and display the list of teachers when the window appears
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.schoolContext = new SchoolDBEntities();
            teachersList.DataContext = this.schoolContext.Teachers;
        }

        // When the user selects a different teacher, fetch and display the students for that teacher
        private void teachersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Find the teacher that has been selected
            this.teacher = teachersList.SelectedItem as Teacher;
            this.schoolContext.LoadProperty<Teacher>(this.teacher, s => s.Students);

            // Find the students for this teacher
            this.studentsInfo = ((IListSource)teacher.Students).GetList();

            // Use databinding to display these students
            studentsList.DataContext = this.studentsInfo;
        }

        #endregion

        public void OnStudentEntitySetHandler(Student student,
            EventArgs e)
        {
            Console.WriteLine("Student set in form: " + student.FirstName + " " + student.LastName);
        }

        private void editStudent(Student student)
        {
            // Use the StudentsForm to display and edit the details of the student
            StudentForm sf = new StudentForm();

            sf.OnStudentEntitySet += OnStudentEntitySetHandler;

            // Set the title of the form and populate the fields on the form with the details of the student           
            sf.Title = "Edit Student Details";

            sf.StudentEntity = student;

            // Display the form
            if (sf.ShowDialog().Value)
            {
                // When the user closes the form, copy the details back to the student

                student = sf.StudentEntity;
                /*
                student.FirstName = sf.firstName.Text;
                student.LastName = sf.lastName.Text;
                student.DateOfBirth = DateTime.Parse(sf.dateOfBirth.Text);
                */

                // Enable saving (changes are not made permanent until they are written back to the database)
                saveChanges.IsEnabled = true;
            }

            sf.OnStudentEntitySet -= OnStudentEntitySetHandler;
        }

        private void addNewStudent()
        {
            // Set the title of the form to indicate which class the student will be added to (the class for the currently selected teacher)
            // Use the StudentsForm to get the details of the student from the user
            StudentForm sf = new StudentForm();

            sf.Title = "New Student for Class " + teacher.Class;

            // Display the form and get the details of the new student
            if (sf.ShowDialog().Value)
            {
                // When the user closes the form, retrieve the details of the student from the form
                // and use them to create a new Student object


                Student student = sf.StudentEntity;

                // Assign the new student to the current teacher
                this.teacher.Students.Add(student);

                // Add the student to the list displayed on the form
                this.studentsInfo.Add(student);

                // Enable saving (changes are not made permanent until they are written back to the database)
                saveChanges.IsEnabled = true;
            }

        }

        private void removeStudent(Student student)
        {
            // Prompt the user to confirm that the student should be removed
            MessageBoxResult response = MessageBox.Show(
                String.Format("Remove {0}", student.FirstName + " " + student.LastName),
                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question,
                MessageBoxResult.No);

            // If the user clicked Yes, remove the student from the database
            if (response == MessageBoxResult.Yes)
            {
                this.schoolContext.Students.DeleteObject(student);

                // Enable saving (changes are not made permanent until they are written back to the database)
                saveChanges.IsEnabled = true;
            }

        }

        // When the user presses a key, determine whether to add a new student to a class, remove a student from a class, or modify the details of a student
        private void studentsList_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                // If the user pressed Enter, edit the details for the currently selected student
                case Key.Enter:

                    Student student = this.studentsList.SelectedItem as Student;

                    editStudent(student);

                    break;

                // If the user pressed Insert, add a new student
                case Key.Insert:

                    addNewStudent();

                    break;

                // If the user pressed Delete, remove the currently selected student
                case Key.Delete:
                    student = this.studentsList.SelectedItem as Student;

                    removeStudent(student);

                    break;
            }
        }

        private void studentsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Student student = this.studentsList.SelectedItem as Student;

            editStudent(student);
        }

        // Save changes back to the database and make them permanent
        private void saveChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                schoolContext.SaveChanges();

                saveChanges.IsEnabled = false;
            }
            catch (OptimisticConcurrencyException)
            {
                this.schoolContext.Refresh(RefreshMode.StoreWins, schoolContext.Students);
                this.schoolContext.SaveChanges();
            }
            catch (UpdateException uEx)
            {
                MessageBox.Show(uEx.Message, "Error saving!");
                this.schoolContext.Refresh(RefreshMode.StoreWins, schoolContext.Students);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error saving changes");
                this.schoolContext.Refresh(RefreshMode.ClientWins, schoolContext.Students);
            }
        }
    }

    [ValueConversion(typeof(string), typeof(Decimal))]
    class AgeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture)
        {
            // Convert the date of birth provided in the value parameter and convert to the age of the student in years
            if (value != null)
            {
                DateTime studentDateOfBirth = (DateTime)value;
                TimeSpan difference = DateTime.Now.Subtract(studentDateOfBirth);
                int ageInYears = (int)(difference.Days / 365.25);
                return ageInYears.ToString();
            }
            else
            {
                return "";
            }
        }

        #region Predefined code

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
