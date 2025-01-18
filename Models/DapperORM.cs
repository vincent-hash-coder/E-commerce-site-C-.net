using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace E_Commerce_Project_CRUD_Dapper.Models
{
    public static class DapperORM
    {
        private static string Connectionstring = @"Data Source=(localdb)\Local;Initial Catalog=ECommerceDB;Integrated Security=True";
        // Getting a full list of value from the DB
        public static List<T> ReturnList<T>(string ProcedureName, DynamicParameters Parameters =null)
        {
            using (SqlConnection con = new SqlConnection(Connectionstring))
            {
                con.Open();
                return (List<T>) (con.Query<T>(ProcedureName, Parameters, commandType: CommandType.StoredProcedure));
            }
                
        }
        // Passing a parameter and give a single return from the DB
        public static T ReturnSingleRow<T>(string procedureName, DynamicParameters parameters = null)
        {
            using (SqlConnection con = new SqlConnection(Connectionstring))
            {
                con.Open();
                return con.QuerySingleOrDefault<T>(procedureName, parameters, commandType: CommandType.StoredProcedure);
            }
        }
        // For Admins update and delect action
        public static void ReturnNothing(string storedProcedureName, DynamicParameters param = null)
        {
            using (SqlConnection con = new SqlConnection(Connectionstring))
            {
                con.Execute(storedProcedureName, param, commandType: CommandType.StoredProcedure);
            }
        }
    }
}