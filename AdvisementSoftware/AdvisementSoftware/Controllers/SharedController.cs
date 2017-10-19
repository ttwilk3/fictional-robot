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
            try
            {
                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                string userID = getUserID();

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

        public void deleteCourseFromProfile(Course item)
        {
            try
            {
                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                string userID = getUserID();

                string delCom = "DELETE FROM USERPROFILE WHERE UserID = '" + userID + "' AND CourseID = '" + item.CourseID + "'";

                SqlCommand del = new SqlCommand(delCom);

                del.Connection = new SqlConnection(connectionString);

                del.Connection.Open();

                del.ExecuteNonQuery();

            }
            catch
            {

            }
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
            DataTable table = new DataTable();
            string findProfile = "SELECT USERPROFILE.CourseID, COURSES.CourseName, COURSES.CreditHours, USERPROFILE.Comment FROM ASPNETUSERS INNER JOIN USERPROFILE ON USERPROFILE.UserID = '";
            try
            {
                string userID = getUserID();

                findProfile += userID + "' INNER JOIN COURSES ON USERPROFILE.CourseID = COURSES.CourseID";

                string path = System.IO.Directory.GetCurrentDirectory();

                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                SqlDataAdapter dataAdapter = new SqlDataAdapter(findProfile, connectionString);

                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);

                dataAdapter.Fill(table);

                List<UserProfile> ups = new List<UserProfile>();
                foreach (DataRow r in table.Rows)
                {
                    UserProfile up = new UserProfile();
                    foreach (DataColumn c in table.Columns)
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

        public string getUserID()
        {
            string userName = User.Identity.Name;
            DataTable table = new DataTable();
            string findID = "SELECT Id FROM ASPNETUSERS WHERE Email = '" + userName + "'";
            try
            {
                string path = System.IO.Directory.GetCurrentDirectory();

                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                SqlDataAdapter dataAdapter = new SqlDataAdapter(findID, connectionString);

                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);

                table.Locale = System.Globalization.CultureInfo.InvariantCulture;

                dataAdapter.Fill(table);

                return table.Rows[0][0].ToString();
            }
            catch
            {
                return "";
            }
        }
    }
}