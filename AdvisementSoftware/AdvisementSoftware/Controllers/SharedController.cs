using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json.Serialization;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace AdvisementSoftware.Controllers
{
    public class SharedController : Controller
    {

        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public class Catalog
        {
            public string Year { get; set; }
        }

        public class Prerequisite
        {
            public string CourseID { get; set; }
            public string Prereq { get; set; } 

            public string BooleanExp { get; set; }

            public bool PrereqMet { get; set; }

            public Prerequisite(string _courseID, string _prereq)
            {
                CourseID = _courseID;
                Prereq = _prereq;
                BooleanExp = _prereq;
            }
        }

        public string generateSchedule(Catalog catalogSelection)
        {
            DataTable catalog = new DataTable();
            DataTable userProfile = new DataTable();
            List<Prerequisite> catalogClasses = new List<Prerequisite>();
            List<string> userClasses = new List<string>();

            try
            {
                string userID = getUserID();

                /*Get Catalog*/

                //string getCatalog = "SELECT * FROM CATALOGS WHERE CatalogYear = '" + catalogSelection.Year + "'";
                string getCatalog = "SELECT CATALOGS.CourseID, COURSES.Prereq FROM CATALOGS INNER JOIN COURSES ON CATALOGS.CourseID = COURSES.CourseID";

                SqlDataAdapter dataAdapter = new SqlDataAdapter(getCatalog, connectionString);

                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);

                catalog.Locale = System.Globalization.CultureInfo.InvariantCulture;

                dataAdapter.Fill(catalog);

                /*Get User*/

                string getUser = "SELECT CourseID FROM USERPROFILE WHERE UserID = '" + userID + "'";

                dataAdapter = new SqlDataAdapter(getUser, connectionString);

                commandBuilder = new SqlCommandBuilder(dataAdapter);

                userProfile.Locale = System.Globalization.CultureInfo.InvariantCulture;

                dataAdapter.Fill(userProfile);

                if (catalog.Rows.Count > 0 && userProfile.Rows.Count > 0)
                {
                    foreach (DataRow r in catalog.Rows)
                    {
                        catalogClasses.Add(new Prerequisite(r["CourseID"].ToString(), r["Prereq"].ToString()));
                    }

                    foreach (DataRow r in userProfile.Rows)
                    {
                        userClasses.Add(r["CourseID"].ToString());
                    }

                    for (int i = catalogClasses.Count - 1; i >= 0; i--)
                    {
                        foreach (string s in userClasses)
                        {
                            if (s.Equals(catalogClasses[i].CourseID))
                            {
                                catalogClasses.RemoveAt(i);
                                break;
                            }
                        }
                    }

                    for (int i = 0; i < catalogClasses.Count; i++)
                    {
                        Match result = Regex.Match(catalogClasses[i].Prereq, @"([A-Z][A-Z][A-Z][A-Z][0-9][0-9][0-9][0-9])");

                        List<string> matches = new List<string>();
                        Match temp = result;
                        while (temp.Success == true)
                        {
                            matches.Add(temp.Value);
                            temp = temp.NextMatch();
                        }

                        foreach (string s in matches)
                        {
                            if (userClasses.Contains(s))
                            {
                                //expression.Replace(s, "true");
                                catalogClasses[i].BooleanExp = Regex.Replace(catalogClasses[i].BooleanExp, s, "True");
                            }
                            else
                            {
                                catalogClasses[i].BooleanExp = Regex.Replace(catalogClasses[i].BooleanExp, s, "False");
                            }
                        }

                        if (catalogClasses[i].BooleanExp.Length > 0)
                            catalogClasses[i].BooleanExp = catalogClasses[i].BooleanExp.Replace("&", " and ").Replace("|", " or ").Replace("&&", " and ").Replace("||", " or ").Replace("!true", "False").Replace("!false", "True").Replace("!True", "False").Replace("!False", "True");
                        else
                            catalogClasses[i].BooleanExp = "(True)";

                        DataTable dt = new DataTable();
                        var val = dt.Compute(catalogClasses[i].BooleanExp, string.Empty);
                        catalogClasses[i].PrereqMet = Convert.ToBoolean(val);
                    }

                    //Console.WriteLine(catalogClasses);
                    string json = JsonConvert.SerializeObject(catalogClasses);
                    return json;
                }
            }
            catch (Exception e)
            {
                return null;
            }
            return null;
        }

        public void addCourse(Course json)
        {
            //Course newCourse = JsonConvert.DeserializeObject<Course>((string)json);
            try
            {
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