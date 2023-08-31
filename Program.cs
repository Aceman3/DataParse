using System;
using System.Data;
using System.IO;
using DataParse;
using Microsoft.Data.SqlClient;


namespace ReadATextFile
{
    class Program
    {
        // Folder for now, until have actual file location
        static readonly string rootFolder = @"C:\Users\hfarrell\Desktop\TestTextFiles\";

        static void Main(string[] args)
        {
            // Opens and reads every file in the given folder
            foreach (string file in Directory.EnumerateFiles(rootFolder, "*.txt"))
            {
                // File being read from folder
                if (File.Exists(file))
                {
                    // Read entire text file content in one string
                    string text = File.ReadAllText(file);

                    // Split string into an array for parsing
                    text.ToArray();
                    string[] values = text.Split(',');

                    // Test console print.
                    Console.WriteLine("Text file being read: " + text + "\n");


                    // Parse the text into Product object to be saved to SQL database
                    // TODO Figure out the structure of the text/log file
                    Product product = new Product();
                    product.GoodBad = values[0].ToString();
                    product.DateTime = DateTime.Parse(values[1].ToString());
                    product.Color = values[2].ToString();
                    product.Processed = Boolean.Parse(values[3].ToString());
                    product.PercentGood = Decimal.Parse(values[4].ToString());
                    // Test console print
                    Console.WriteLine(String.Format("Here is the object being saved to the database: \nGood/Bad?: {0}\nDate/Time: {1}\nColor: {2}\n" +
                        "Processed: {3}\nPercentageGood: {4}\n\n", product.GoodBad, product.DateTime, product.Color, product.Processed, product.PercentGood));


                    // Create SQL connection 


                    using (SqlConnection conn = new SqlConnection(@"Data Source=nc-dc1;Initial Catalog=MockApp;User Id=PIFUser;Password=UserPIF;MultipleActiveResultSets=True"))
                    {
                        string insertString = "INSERT into dbo.Table_1 (GoodBad,DateTime,Color,Processed,PercentGood) VALUES(@GoodBad,@DateTime,@Color,@Processed,@PercentGood)";

                        using (SqlCommand cmd = new SqlCommand(insertString))
                        {
                            cmd.Connection = conn;
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.Add("@GoodBad", SqlDbType.VarChar, 10).Value = product.GoodBad;
                            cmd.Parameters.Add("@DateTime", SqlDbType.DateTime).Value = product.DateTime;
                            cmd.Parameters.Add("@Color", SqlDbType.VarChar, 10).Value = product.Color;
                            cmd.Parameters.Add("@Processed", SqlDbType.VarChar, 10).Value = product.Processed;
                            cmd.Parameters.Add("@PercentGood", SqlDbType.Decimal, 3).Value = product.PercentGood;
                            try
                            {
                                conn.Open();
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.ToString());
                            }
                            finally
                            {
                                cmd.Dispose();
                                conn.Close();
                            }
                        }
                    }




                    // Create and set parameters for sending to DB
                    //SqlParameter parameterGoodBad = new SqlParameter("@GoodBad", SqlDbType.VarChar, 10);
                    //SqlParameter parameterDateTime = new SqlParameter("@DateTime", SqlDbType.DateTime);
                    //SqlParameter parameterColor = new SqlParameter("@Color", SqlDbType.VarChar, 10);
                    //SqlParameter parameterProcessed = new SqlParameter("@Processed", SqlDbType.VarChar, 10);
                    //SqlParameter parameterPercentGood = new SqlParameter("@PercentGood", SqlDbType.Decimal, 18,0);

                    //parameterGoodBad.Value = product.GoodBad;
                    //parameterDateTime.Value = product.DateTime;
                    //parameterColor.Value = product.Color;
                    //parameterProcessed.Value = product.Processed;
                    //parameterPercentGood.Value = product.PercentGood;

                    // Add parameters to command
                    //cmd.Parameters.Add(parameterGoodBad);
                    //cmd.Parameters.Add(parameterDateTime);
                    //cmd.Parameters.Add(parameterColor);
                    //cmd.Parameters.Add(parameterProcessed);
                    //cmd.Parameters.Add(parameterPercentGood);

                    // Try sending to DB
                    //try
                    //{
                    //    conn.Open();
                    //    cmd.ExecuteNonQuery();
                    //}
                    //catch (Exception ex)
                    //{
                    //    throw new Exception(ex.ToString());
                    //}
                    //finally
                    //{
                    //    cmd.Dispose();
                    //    conn.Close();
                    //}


                    // Delete file from folder once finished processing
                    //File.Delete(file);


                }



            }

            Console.ReadKey();

        }
    }
}