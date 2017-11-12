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
        static Random rand = new Random();

        public class Catalog
        {
            public string Year { get; set; }
            public int ElectiveHours { get; set; }
        }

        public class Prerequisite
        {
            public string CourseID { get; set; }

            public string CourseName { get; set; }
            public string Prereq { get; set; } 

            public string BooleanExp { get; set; }

            public bool PrereqMet { get; set; }

            public int CreditHours { get; set; }

            public float CourseWeightNumber { get; set; }

            public double Weight { get; set; }

            public Prerequisite(string _courseID, string _courseName, string _prereq, int _CreditHours)
            {
                CourseID = _courseID;
                CourseName = _courseName;
                Prereq = _prereq;
                BooleanExp = _prereq;
                CreditHours = _CreditHours;
            }
        }

        public class FullCatalog
        {
            public string CourseID { get; set; }
            public string CourseName { get; set; }
            public int CreditHours { get; set; }
            public string Area { get; set; }
        }

        public string getCatalogYears()
        {
            DataTable catalog = new DataTable();
            try
            {
                string getCatalogYear = "SELECT DISTINCT CatalogYear FROM CATALOGS";

                SqlDataAdapter dataAdapter = new SqlDataAdapter(getCatalogYear, connectionString);

                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);

                catalog.Locale = System.Globalization.CultureInfo.InvariantCulture;

                dataAdapter.Fill(catalog);

                List<Catalog> years = new List<Catalog>();

                foreach (DataRow r in catalog.Rows)
                {
                    Catalog year = new Catalog();
                    foreach (DataColumn c in catalog.Columns)
                    {
                        if (c.ColumnName.Equals("CatalogYear"))
                            year.Year = r[c.ColumnName.ToString()].ToString();
                    }
                    years.Add(year);
                }

                string json = JsonConvert.SerializeObject(years);
                return json;
            }
            catch
            {
                return "";
            }
        }

        public string getCatalog(Catalog catalogSelection)
        {
            DataTable catalog = new DataTable();
            try
            {
                string getCatalog = "SELECT CATALOGS.CourseID, COURSES.CreditHours, COURSES.CourseName, CATALOGS.Area FROM CATALOGS INNER JOIN COURSES ON CATALOGS.CourseID = COURSES.CourseID WHERE CatalogYear = '" + catalogSelection.Year + "'";

                SqlDataAdapter dataAdapter = new SqlDataAdapter(getCatalog, connectionString);

                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);

                catalog.Locale = System.Globalization.CultureInfo.InvariantCulture;

                dataAdapter.Fill(catalog);

                List<FullCatalog> fullCata = new List<FullCatalog>();

                foreach (DataRow r in catalog.Rows)
                {
                    FullCatalog course = new FullCatalog();
                    foreach (DataColumn c in catalog.Columns)
                    {
                        if (c.ColumnName.Equals("CourseID"))
                            course.CourseID = r[c.ColumnName.ToString()].ToString();
                        else if (c.ColumnName.Equals("CourseName"))
                            course.CourseName = r[c.ColumnName.ToString()].ToString();
                        else if (c.ColumnName.Equals("Area"))
                            course.Area = r[c.ColumnName.ToString()].ToString();
                        else if (c.ColumnName.Equals("CreditHours"))
                        {
                            int temp = 0;
                            Int32.TryParse(r[c.ColumnName.ToString()].ToString(), out temp);
                            course.CreditHours = temp;
                        }
                    }
                    fullCata.Add(course);
                }

                string json = JsonConvert.SerializeObject(fullCata);
                return json;
            }
            catch
            {
                return null;
            }
        }

        public string generateSchedule(Catalog catalogSelection)
        {
            DataTable catalog = new DataTable();
            DataTable userProfile = new DataTable();
            DataTable eleHours = new DataTable();
            List<Prerequisite> catalogClasses = new List<Prerequisite>();
            List<string> userClasses = new List<string>();
            List<List<string>> schedule = new List<List<string>>(); 

            try
            {
                string userID = getUserID();

                /*Get Catalog*/

                //string getCatalog = "SELECT * FROM CATALOGS WHERE CatalogYear = '" + catalogSelection.Year + "'";
                string getCatalog = "SELECT CATALOGS.CourseID, COURSES.Prereq, COURSES.CreditHours, COURSES.CourseName FROM CATALOGS INNER JOIN COURSES ON CATALOGS.CourseID = COURSES.CourseID WHERE CatalogYear = '" + catalogSelection.Year + "'";

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

                /*Get Elective Hours*/

                string getElectiveHours = "SELECT ElectiveHours FROM CATALOGELECTIVEHOURS WHERE CATALOGYEAR = '" + catalogSelection.Year + "'";

                dataAdapter = new SqlDataAdapter(getElectiveHours, connectionString);

                commandBuilder = new SqlCommandBuilder(dataAdapter);

                eleHours.Locale = System.Globalization.CultureInfo.InvariantCulture;

                dataAdapter.Fill(eleHours);

                int requiredElectiveHours = 0;

                Int32.TryParse(eleHours.Rows[0][0].ToString(), out requiredElectiveHours);

                if (catalogSelection.ElectiveHours < requiredElectiveHours)
                {
                    int hoursRemaining = requiredElectiveHours - catalogSelection.ElectiveHours;

                    while (hoursRemaining != 0)
                    {
                        if (hoursRemaining - 3 >= 0)
                        {
                            Prerequisite temp3 = new Prerequisite("ELEC1003", "Elective 3 Hour", "", 3);
                            temp3.CourseWeightNumber = rand.Next(1, 5) * 1000;
                            catalogClasses.Add(temp3);
                            hoursRemaining -= 3;
                        }
                        else
                        {
                            Prerequisite temp1 = new Prerequisite("ELEC1001", "Elective 1 Hour", "", 1);
                            temp1.CourseWeightNumber = rand.Next(1, 5) * 1000;
                            catalogClasses.Add(temp1);
                            hoursRemaining -= 1;
                        }

                    }
                }

                if (catalog.Rows.Count > 0 && userProfile.Rows.Count > 0)
                {
                    foreach (DataRow r in catalog.Rows)
                    {
                        Prerequisite temp = new Prerequisite(r["CourseID"].ToString(), r["CourseName"].ToString(), r["Prereq"].ToString(), Int32.Parse(r["CreditHours"].ToString()));
                        temp.CourseWeightNumber = Int32.Parse(temp.CourseID.Substring(4));
                        catalogClasses.Add(temp);
                    }

                    foreach (DataRow r in userProfile.Rows)
                    {
                        userClasses.Add(r["CourseID"].ToString());
                    }

                    List<string> allNeededPrereqs = new List<string>();

                    while (catalogClasses.Count > 0)
                    {
                        //catalogClasses = catalogClasses.OrderBy(item => rand.Next()).ToList();
                        catalogClasses.Sort((x, y) => y.CourseWeightNumber.CompareTo(x.CourseWeightNumber));
                        filter(ref catalogClasses, ref userClasses, ref allNeededPrereqs, ref schedule);
                    }
                    
                    //Console.WriteLine(schedule);
                    string json = JsonConvert.SerializeObject(schedule);
                    return json;
                }
            }
            catch (Exception e)
            {
                return null;
            }
            return null;
        }

        public void filter(ref List<Prerequisite> catalogClasses, ref List<string> userClasses, ref List<string> allNeededPrereqs, ref List<List<string>> schedule)
        {
            for (int i = catalogClasses.Count - 1; i >= 0; i--)
            {
                if (catalogClasses[i].CourseID.Contains("WELL") || catalogClasses[i].CourseID.Contains("SOSC"))
                {
                    catalogClasses[i].CourseWeightNumber = rand.Next(1, 4) * 1000;
                }
                else if (catalogClasses[i].CourseID.Contains("CSCI3"))
                    catalogClasses[i].CourseWeightNumber = rand.Next(2, 4) * 1000;
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
                if (catalogClasses[i].Prereq.Length > 0)
                    catalogClasses[i].BooleanExp = catalogClasses[i].Prereq;
                else
                    catalogClasses[i].BooleanExp = "(True)";

                Match result = Regex.Match(catalogClasses[i].Prereq, @"([A-Z][A-Z][A-Z][A-Z][0-9][0-9][0-9][0-9])");

                List<string> matches = new List<string>();
                Match temp = result;
                while (temp.Success == true)
                {
                    matches.Add(temp.Value);
                    allNeededPrereqs.Add(temp.Value);
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
            allNeededPrereqs = allNeededPrereqs.Distinct().ToList();

            for (int i = allNeededPrereqs.Count - 1; i >= 0; i--)
            {
                if (userClasses.Contains(allNeededPrereqs[i]))
                {
                    allNeededPrereqs.RemoveAt(i);
                }
            }

            /* Next Step Build Schedule */
            List<string> semester = new List<string>();
            int creditHours = 0;
            for (int i = allNeededPrereqs.Count - 1; i >= 0; i--)
            {
                for (int j = catalogClasses.Count - 1; j >= 0; j--)
                {
                    if (catalogClasses[j].CourseID.Equals(allNeededPrereqs[i]) && catalogClasses[j].PrereqMet == true && creditHours <= 15)
                    {
                        creditHours += catalogClasses[i].CreditHours;
                        semester.Add(allNeededPrereqs[i] + " " + catalogClasses[j].CourseName);
                        userClasses.Add(allNeededPrereqs[i]);
                        allNeededPrereqs.RemoveAt(i);
                        catalogClasses.RemoveAt(j);
                        break;
                    }
                }
            }

            if (creditHours < 15)
            {
                for (int i = catalogClasses.Count - 1; i >= 0; i--)
                {
                    if (catalogClasses[i].PrereqMet == true)
                    {
                        creditHours += catalogClasses[i].CreditHours;
                        semester.Add(catalogClasses[i].CourseID + " " + catalogClasses[i].CourseName);
                        if (catalogClasses[i].CourseID.Contains("ELEC") != true)
                            userClasses.Add(catalogClasses[i].CourseID);
                        catalogClasses.RemoveAt(i);
                    }
                    if (creditHours >= 15)
                    {
                        break;
                    }
                }
            }
            semester.Insert(0, "Credit Hours " + creditHours.ToString());
            schedule.Add(semester);
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

        public string getAreas()
        {
            List<string> areas = new List<string>();
            DataTable catalog = new DataTable();
            try
            {
                string getCatalogArea = "SELECT DISTINCT Area FROM CATALOGS";

                SqlDataAdapter dataAdapter = new SqlDataAdapter(getCatalogArea, connectionString);

                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);

                catalog.Locale = System.Globalization.CultureInfo.InvariantCulture;

                dataAdapter.Fill(catalog);

                foreach (DataRow r in catalog.Rows)
                {
                    string temp = "";
                    foreach (DataColumn c in catalog.Columns)
                    {
                        if (c.ColumnName.Equals("Area"))
                            temp = r[c.ColumnName.ToString()].ToString();
                    }
                    areas.Add(temp);
                }

                string json = JsonConvert.SerializeObject(areas);
                return json;
            }
            catch
            {
                return "";
            }
        }

        public class CatalogDelete
        {
            public string CatalogYear { get; set; }

            public string Area { get; set; }
            public string CourseID { get; set; }
        }

        public void deleteCourseFromCatalog(CatalogDelete item)
        {
            try
            {

                string delCom = "DELETE FROM CATALOGS WHERE CatalogYear = '" + item.CatalogYear + "' AND CourseID = '" + item.CourseID + "'";

                SqlCommand del = new SqlCommand(delCom);

                del.Connection = new SqlConnection(connectionString);

                del.Connection.Open();

                del.ExecuteNonQuery();

            }
            catch
            {

            }
        }

        public void addCourseToCatalog(CatalogDelete course)
        {
            try
            {

                string insertCommand = "INSERT INTO CATALOGS (CatalogYear, CourseID, Area) VALUES ('" + course.CatalogYear + "', '" + course.CourseID + "', '" + course.Area + "')";

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

            public string Prereq { get; set; }
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
                    if (course.CourseID.Contains("ELEC") != true)
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

        public void addNewCourse(Course c)
        {
            try
            {
                string insertCommand = "INSERT INTO COURSES (CourseID, CourseName, SemesterOffered, CreditHours, Prereq) VALUES ('" + c.CourseID + "', '" + c.CourseName + "', '" + c.SemesterOffered + "', '" + c.CreditHours + "', '" + c.Prereq + "')";

                SqlCommand ins = new SqlCommand(insertCommand);

                ins.Connection = new SqlConnection(connectionString);

                ins.Connection.Open();

                ins.ExecuteNonQuery();
            }
            catch (Exception e)
            {

            }
        }

        public class UserProfile
        {
            public string CourseID { get; set; }
            public string CourseName { get; set; }
            public int CreditHours { get; set; }
            public string Comment { get; set; }
        }

        public class SearchUser
        {
            public string userName { get; set; }
        }
        public string getProfile(SearchUser u)
        {
            DataTable table = new DataTable();
            string findProfile = "SELECT USERPROFILE.CourseID, COURSES.CourseName, COURSES.CreditHours, USERPROFILE.Comment FROM ASPNETUSERS INNER JOIN USERPROFILE ON USERPROFILE.UserID = '";
            try
            {
                string userID;

                if (u.userName == null || u.userName == "")
                    userID = getUserID();
                else
                    userID = getUserID(u.userName);

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

        public class NewCatalog
        {
            public string CatalogYearCopy { get; set; }
            public bool CopyCatalog { get; set; }
            public string Year { get; set; }
        }
        public void addNewCatalog(NewCatalog catalog)
        {
            DataTable copyCat = new DataTable();
            try
            {
                if (catalog.CopyCatalog == true)
                {
                    string getCatalog = "SELECT CatalogYear, CourseID, Area FROM CATALOGS WHERE CatalogYear = '" + catalog.CatalogYearCopy + "'";

                    SqlDataAdapter dataAdapter = new SqlDataAdapter(getCatalog, connectionString);

                    SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);

                    copyCat.Locale = System.Globalization.CultureInfo.InvariantCulture;

                    dataAdapter.Fill(copyCat);

                    List<CatalogDelete> courses = new List<CatalogDelete>();

                    foreach (DataRow r in copyCat.Rows)
                    {
                        CatalogDelete course = new CatalogDelete();
                        foreach (DataColumn c in copyCat.Columns)
                        {
                            if (c.ColumnName.Equals("CatalogYear"))
                                course.CatalogYear = catalog.Year;
                            else if (c.ColumnName.Equals("CourseID"))
                                course.CourseID = r[c.ColumnName.ToString()].ToString();
                            else if (c.ColumnName.Equals("Area"))
                                course.Area = r[c.ColumnName.ToString()].ToString();
                        }
                        courses.Add(course);
                    }

                    foreach (CatalogDelete cd in courses)
                    {
                        string newCatalog = "INSERT INTO CATALOGS (CatalogYear, CourseID, Area) VALUES ('" + cd.CatalogYear + "', '" + cd.CourseID + "', '" + cd.Area + "')";

                        SqlCommand ins = new SqlCommand(newCatalog);

                        ins.Connection = new SqlConnection(connectionString);

                        ins.Connection.Open();

                        ins.ExecuteNonQuery();
                    }
                }
                else if (catalog.CopyCatalog == false)
                {
                    string newCatalog = "INSERT INTO CATALOGS (CatalogYear, CourseID, Area) VALUES ('" + catalog.Year + "', 'ENGL1101', 'A')" ;

                    SqlCommand ins = new SqlCommand(newCatalog);

                    ins.Connection = new SqlConnection(connectionString);

                    ins.Connection.Open();

                    ins.ExecuteNonQuery();
                }
            }
            catch
            {

            }
        }

        public string getGeneralElectiveHours(Catalog catalogSelection)
        {
            string userName = User.Identity.Name;
            DataTable table = new DataTable();
            string findHours = "SELECT ElectiveHours FROM CATALOGELECTIVEHOURS WHERE CatalogYear = '" + catalogSelection.Year + "'";
            try
            {
                string path = System.IO.Directory.GetCurrentDirectory();

                SqlDataAdapter dataAdapter = new SqlDataAdapter(findHours, connectionString);

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

        public string getUserID(string userName)
        {
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