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
    }
}