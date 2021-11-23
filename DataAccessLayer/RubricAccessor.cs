﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessInterFaces;
using DataObjects;
using System.Data;

namespace DataAccessLayer
{
    public class RubricAccessor : IRubricAccessor
    {
        public Rubric SelectRubricByRubricID(int rubricID)
        {
            Rubric rubric = null;

            var conn = DBConnection.GetConnection();

            var cmdText = "sp_select_rubric_by_rubric_id";

            var cmd = new SqlCommand(cmdText, conn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@RubricID", SqlDbType.Int);
            cmd.Parameters["@RubricID"].Value = rubricID;

            try
            {
                conn.Open();
                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        rubric = new Rubric()
                        {
                            RubricID = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.GetString(2),
                            DateCreated = reader.GetDateTime(3),
                            DateUpdated = reader.GetDateTime(4),
                            ScoreTypeID = reader.GetString(5),
                            RubricCreator = new User() {
                                UserID = reader.GetString(6),
                                GivenName = reader.GetString(7),
                                FamilyName = reader.GetString(8),
                                Active = reader.GetBoolean(9),
                                Roles = new List<string>()
                            },
                            Active = reader.GetBoolean(10)
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }
            return rubric;
        }

        public List<Rubric> SelectRubrics()
        {
            List<Rubric> rubrics = new List<Rubric>();

            // connection
            var conn = DBConnection.GetConnection();

            // command text
            var cmdText = "sp_select_active_rubrics";

            // command
            var cmd = new SqlCommand(cmdText, conn);

            // command type
            cmd.CommandType = CommandType.StoredProcedure;

            try
            {
                conn.Open();
                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Rubric rubric = new Rubric()
                        {
                            RubricID = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.GetString(2),
                            DateCreated = reader.GetDateTime(3),
                            DateUpdated = reader.GetDateTime(4),
                            ScoreTypeID = reader.GetString(5), 
                            RubricCreator = new User() { 
                                UserID = reader.GetString(6),
                                GivenName = reader.GetString(7),
                                FamilyName = reader.GetString(8)
                            }
                        };

                        rubrics.Add(rubric);
                    }
                }
                else
                {
                    throw new ApplicationException("Rubric not found");
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                conn.Close();
            }

            return rubrics;
        }
    }
}
