using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json.Serialization;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;

namespace AdvisementSoftware.Controllers
{
    public class SharedController : Controller
    {

        public void addCourse(Course json)
        {
            //Course newCourse = JsonConvert.DeserializeObject<Course>((string)json);
            string userName = User.Identity.Name;
            string findID = "SELECT Id FROM ASPNETUSERS WHERE Email = '" + userName + "'";
            DataTable table = new DataTable();

            try
            {
                string path = System.IO.Directory.GetCurrentDirectory();

                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                SqlDataAdapter dataAdapter = new SqlDataAdapter(findID, connectionString);

                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);

                table.Locale = System.Globalization.CultureInfo.InvariantCulture;

                dataAdapter.Fill(table);

                string userID = table.Rows[0][0].ToString();

                string insertCommand = "INSERT INTO USERPROFILE (UserID, CourseID) VALUES ('" + userID + "', '" + json.CourseID + "')";

                SqlCommand ins = new SqlCommand(insertCommand);

                ins.Connection = new SqlConnection(connectionString);

                ins.Connection.Open();

                ins.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
            }
        }

        public void deleteCourseFromProfile()
        {

        }
       public class Course
        {
            public string CourseID { get; set; }
            public string CourseName { get; set; }
            public string SemesterOffered { get; set; }
            public int CreditHours { get; set; }
        }
        public string getCourses()
        {
            DataTable table = new DataTable();
            string selectCommand = "SELECT * FROM COURSES";
            try
            {
                string path = System.IO.Directory.GetCurrentDirectory();

                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                SqlDataAdapter dataAdapter = new SqlDataAdapter(selectCommand, connectionString);

                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);

                table.Locale = System.Globalization.CultureInfo.InvariantCulture;

                dataAdapter.Fill(table);

                List<Course> courses = new List<Course>();

                foreach(DataRow r in table.Rows)
                {
                    Course course = new Course();
                    foreach (DataColumn c in table.Columns)
                    {
                        if (c.ColumnName.Equals("CourseID"))
                            course.CourseID = r[c.ColumnName.ToString()].ToString();
                        else if (c.ColumnName.Equals("CourseName"))
                            course.CourseName = r[c.ColumnName.ToString()].ToString();
                        else if (c.ColumnName.Equals("SemesterOffered"))
                            course.SemesterOffered = r[c.ColumnName.ToString()].ToString();
                        else if (c.ColumnName.Equals("CreditHours"))
                        {
                            int temp = 0;
                            Int32.TryParse(r[c.ColumnName.ToString()].ToString(), out temp);
                            course.CreditHours = temp;
                        }
                    }
                    courses.Add(course);
                }

                string json = JsonConvert.SerializeObject(courses);
                return json;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public class UserProfile
        {
            public string CourseID { get; set; }
            public string CourseName { get; set; }
            public int CreditHours { get; set; }
            public string Comment { get; set; }
        }

        public string getProfile()
        {
            string userName = User.Identity.Name;
            DataTable table = new DataTable();
            DataTable table2 = new DataTable();
            string findID = "SELECT Id FROM ASPNETUSERS WHERE Email = '" + userName + "'";
            string findProfile = "SELECT USERPROFILE.CourseID, COURSES.CourseName, COURSES.CreditHours, USERPROFILE.Comment FROM ASPNETUSERS INNER JOIN USERPROFILE ON USERPROFILE.UserID = '";
            try
            {
                string path = System.IO.Directory.GetCurrentDirectory();

                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                SqlDataAdapter dataAdapter = new SqlDataAdapter(findID, connectionString);

                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);

                table.Locale = System.Globalization.CultureInfo.InvariantCulture;

                dataAdapter.Fill(table);

                string userID = table.Rows[0][0].ToString();



                findProfile += userID + "' INNER JOIN COURSES ON USERPROFILE.CourseID = COURSES.CourseID";

                dataAdapter = new SqlDataAdapter(findProfile, connectionString);

                commandBuilder = new SqlCommandBuilder(dataAdapter);

                dataAdapter.Fill(table2);

                List<UserProfile> ups = new List<UserProfile>();
                foreach (DataRow r in table2.Rows)
                {
                    UserProfile up = new UserProfile();
                    foreach (DataColumn c in table2.Columns)
                    {
                        //CourseID
                        //CourseName
                        //CreditHours
                        //Comment
                        if (c.ColumnName.Equals("CourseID"))
                            up.CourseID = r[c.ColumnName.ToString()].ToString();
                        else if (c.ColumnName.Equals("CourseName"))
                            up.CourseName = r[c.ColumnName.ToString()].ToString();
                        else if (c.ColumnName.Equals("CreditHours"))
                        {
                            int temp = 0;
                            Int32.TryParse(r[c.ColumnName.ToString()].ToString(), out temp);
                            up.CreditHours = temp;
                        }
                        else if (c.ColumnName.Equals("Comment"))
                            up.Comment = r[c.ColumnName.ToString()].ToString();

                    }
                    ups.Add(up);
                }

                string json = JsonConvert.SerializeObject(ups);
                return json;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}