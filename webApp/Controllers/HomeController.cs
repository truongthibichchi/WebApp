using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using webApp.Models;
using System.IO;
using System.Data.SqlClient;
using WordTemplate;
using System.Text.RegularExpressions;
using System.Data;

namespace webApp.Controllers
{
    public class HomeController : Controller
    {
        ServiceManagement db = new ServiceManagement();
        public ActionResult NewService()
        {
            return View();
        }

        public ActionResult ServiceList()
        {
            var serviceList = (from s in db.Services where s.isActive == 1 select s).ToArray();
            ViewBag.serviceList = serviceList;
            return View();
        }


        [HttpPost]
        public int SaveServiceAndHeading(List<String> headingList, List<String> mappingList, List<String> lengthList, String serviceName)
        {
            int result = 0;
            try
            {

                Service service = new Service();
                service.serviceName = serviceName;
                service.wordTemplate = serviceName + ".docx";
                service.isActive = 1;
                db.Services.Add(service);
                db.SaveChanges();

                var serviceID = (from s in db.Services where s.serviceName == serviceName select s.id).FirstOrDefault();


                if (headingList.Count > 0)
                {
                    SqlConnection connection =
                        new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ServiceManagement"].ConnectionString);

                    // Writes the mappings for the current service to the mapping table.

                    String csvColumnName = null;
                    String dbColumnName = null;
                    String length = null;
                    for (int i = 0; i < headingList.Count; i++)
                    {
                        connection.Open();
                        string query = "insert into ServiceColumnDbMapping (serviceId,csvColumnName, dbColumnName, length) values (@serviceID, @csvColumnName, @dbColumnName, @length);";
                        csvColumnName = headingList.ElementAt(i);
                        dbColumnName = mappingList.ElementAt(i);
                        length = lengthList.ElementAt(i);

                        SqlCommand cmd = new SqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@serviceID", serviceID);
                        cmd.Parameters.AddWithValue("@csvColumnName", csvColumnName);
                        cmd.Parameters.AddWithValue("@dbColumnName", dbColumnName);
                        cmd.Parameters.AddWithValue("@length", length);

                        result = cmd.ExecuteNonQuery();
                        connection.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Response.Write(e.Message);

            }
            return result;
        }

        [HttpPost]
        public bool SaveWordTemplate(String serviceName, HttpPostedFileBase fileWord)
        {
            bool success = false;
            try
            {
                string extension = Path.GetExtension(fileWord.FileName);
                string fileName = serviceName + extension;
                string filePath = Path.Combine(Server.MapPath("~/FileUpload"), fileName);

                fileWord.SaveAs(filePath);
                success = true;
            }
            catch
            {
                success = false;
            }
            return success;
        }


        [HttpPost]
        public int DeleteService(int serviceID)
        {
            int result = 0;
            try
            {
                SqlConnection connection =
                   new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ServiceManagement"].ConnectionString);

                SqlCommand cmd = new SqlCommand("DeleteService", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@serviceID", serviceID);
                cmd.Parameters["@serviceID"].Direction = ParameterDirection.Input;

                connection.Open();
                result = cmd.ExecuteNonQuery();
                connection.Close();
            }
            catch
            {
            }
            return result;

        }


        [HttpPost]
        public bool FromDatabaseToWordTemplate(string word_template_location = null, string target_table_name = null, string connection_string = null)
        {
            WordTemplateManager.Init(word_template_location, target_table_name, connection_string);
            WordTemplateManager.Run();
            return StaticValues.is_success;
        }

        [HttpPost]
        public JsonResult CSVData(int serviceID, HttpPostedFileBase csvFile, String tableName)
        {
            bool success = true;
            string allowedExtension = ".csv";
            string fileExtension = Path.GetExtension(csvFile.FileName);

            //tableName = tableName.Replace(" ", String.Empty);

            if (fileExtension.Equals(allowedExtension))
            {
                SqlConnection connection =
                    new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["CSVData"].ConnectionString);

                StreamReader reader = new StreamReader(csvFile.InputStream);

                string first = reader.ReadLine();
                string[] headings = first.Split(',');

                // A string for building the SQL command to add the new service table
                // Each service needs an identity field which is initialised here.
                string commandBuild = "CREATE TABLE " + tableName + " (" +
                    "id int NOT NULL IDENTITY(1,1) primary key";
                // Adds the fields passed with varchar data type.
                var mapping = (from m in db.ServiceColumnDbMappings where m.serviceId == serviceID select m).ToArray();
                for (int i = 0; i < mapping.Length; i++)
                {  
                    var a = mapping[i];
                    if (mapping[i] != null)
                    {
                        commandBuild += ", " + mapping[i].dbColumnName + " nvarchar(" + mapping[i].length + ")";
                    }
                    else
                    {
                        success = false;
                    }
                }
                commandBuild += ");";

                // Runs the query and then closes the connection.
                try
                {
                    connection.Open();
                    SqlCommand addTable = new SqlCommand(commandBuild, connection);
                    addTable.ExecuteNonQuery();
                    connection.Close();
                }
                catch
                {
                    //ViewBag.Message = "Failed to add table to database." + e.Message;
                    //return RedirectToAction("ServiceList");
                    success = false;
                }

                while (!reader.EndOfStream)
                {
                    // A string to build the insert query.
                    string command = "INSERT INTO " + tableName + " VALUES (\'";
                    string line = reader.ReadLine();

                    // extract the fields
                    Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                    string[] info = CSVParser.Split(line);


                    // clean up the fields (remove " and leading spaces)
                    for (int i = 0; i < info.Length; i++)
                    {
                        info[i] = info[i].TrimStart(' ', '"');
                        info[i] = info[i].TrimEnd('"');
                    }

                    // Loops through the information in the row and adds it to the insert.
                    for (int i = 0; i < info.Length; i++)
                    {
                        // Adds the next item from the list in.
                        command += info[i] + "\'";
                        // Add a comma if the item is not the last one.
                        if ((i + 1) != info.Length)
                            command += ", \'";
                    }
                    command += ");";
                    // Runs the commmand and closes the connection.
                    try
                    {
                        connection.Open();
                        SqlCommand c = new SqlCommand(command, connection);
                        c.ExecuteNonQuery();
                        success = true;
                        connection.Close();

                    }
                    catch (Exception)
                    {
                        //ViewBag.Message = "Failed to add information to the database." + e.Message;
                        //return RedirectToAction("ServiceList");
                        success = false;
                        connection.Close();
                    }
                }
            }
            else
            {
                success = false;
            }
            String wordTemplate = (from s in db.Services where s.id == serviceID select s.wordTemplate).FirstOrDefault();
            string filePath = Path.Combine(Server.MapPath("~/FileUpload"), wordTemplate);
            String connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["CSVData"].ConnectionString;

            if (success)
                success = FromDatabaseToWordTemplate(filePath, tableName, connectionString);

            return Json(new { success = success, fileName = Path.GetFileName(StaticValues.word_template_path), errorMessage = StaticValues.logs });
        }

        [HttpGet]
        [DeleteFileAttribute] //Action Filter, it will auto delete the file after download 
        public FileResult Download()
        {
            return File(StaticValues.word_template_path, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(StaticValues.word_template_path));
        }
    }

    public class DeleteFileAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            filterContext.HttpContext.Response.Flush();

            //delete the file after download
            try
            {
                System.IO.File.Delete(StaticValues.word_template_path);
            }
            catch (Exception)
            {
                // Ignore
            }
            
        }
    }
}