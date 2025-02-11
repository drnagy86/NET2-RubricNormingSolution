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
        public int DeactivateRubricByRubricID(int rubricID)
        {

            int rowsAffected = 0;

            // connection
            var conn = DBConnection.GetConnection();

            string cmdTxt = "sp_deactivate_rubric_by_rubric_id";
            var cmd = new SqlCommand(cmdTxt, conn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@RubricID", rubricID);
                        

            try
            {
                conn.Open();
                rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }
            return rowsAffected;

        }

        public int DeleteRubricByRubricID(int rubricID)
        {
            int rowsAffected = 0;

            // connection
            var conn = DBConnection.GetConnection();

            string cmdTxt = "sp_delete_rubric_by_rubric_id";
            var cmd = new SqlCommand(cmdTxt, conn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@RubricID", rubricID);


            try
            {
                conn.Open();
                rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }
            return rowsAffected;
        }

        public int InsertRubric(string name, string description, string scoreType, string rubricCreator)
        {
            int rowsAffected = 0;

            // connection
            var conn = DBConnection.GetConnection();

            string cmdTxt = "sp_create_rubric";
            var cmd = new SqlCommand(cmdTxt, conn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 50);
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 100);
            cmd.Parameters.Add("@ScoreTypeID", SqlDbType.NVarChar, 50);
            cmd.Parameters.Add("@RubricCreator", SqlDbType.NVarChar, 50);

            cmd.Parameters["@Name"].Value = name;
            cmd.Parameters["@Description"].Value = description;
            cmd.Parameters["@ScoreTypeID"].Value = scoreType;
            cmd.Parameters["@RubricCreator"].Value = rubricCreator;

            try
            {
                conn.Open();
                rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }
            return rowsAffected;
        }

        public int InsertRubric(RubricVM rubric)
        {
            int rowsAffected = 0;
            int newRubricID = 0;
            
            // connection
            var conn = DBConnection.GetConnection();

            string cmdTxt = "sp_create_rubric_with_one_facet";
            var cmd = new SqlCommand(cmdTxt, conn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@RubricName", SqlDbType.NVarChar, 50);
            cmd.Parameters.Add("@RubricDescription", SqlDbType.NVarChar, 100);
            cmd.Parameters.Add("@ScoreTypeID", SqlDbType.NVarChar, 50);
            cmd.Parameters.Add("@RubricCreator", SqlDbType.NVarChar, 50);
            cmd.Parameters.Add("@NumberOfCriteria", SqlDbType.Int);

            cmd.Parameters["@RubricName"].Value = rubric.Name;
            cmd.Parameters["@RubricDescription"].Value = rubric.Description;
            cmd.Parameters["@ScoreTypeID"].Value = rubric.ScoreTypeID;
            cmd.Parameters["@RubricCreator"].Value = rubric.RubricCreator.UserID;
            cmd.Parameters["@NumberOfCriteria"].Value = rubric.NumberOfCriteria;

            cmd.Parameters.Add("@FacetID", SqlDbType.NVarChar, 100);
            cmd.Parameters.Add("@FacetDescription", SqlDbType.NVarChar, 255);
            cmd.Parameters.Add("@FacetType", SqlDbType.NVarChar, 50);


            cmd.Parameters["@FacetID"].Value = rubric.FacetVMs[0].FacetID;
            cmd.Parameters["@FacetDescription"].Value = rubric.FacetVMs[0].Description;
            cmd.Parameters["@FacetType"].Value = rubric.FacetVMs[0].FacetType;

            try
            {
                conn.Open();
                Object result = cmd.ExecuteScalar();
                //Object result = cmd.ExecuteNonQuery();
                newRubricID = (int)result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }

            //add each criteria
            foreach (Criteria criteria in rubric.FacetVMs[0].Criteria)
            {

                string cmdTxt2 = "sp_create_criteria_by_rubric_id_and_facet_id";
                var cmd2 = new SqlCommand(cmdTxt2, conn);

                cmd2.CommandType = CommandType.StoredProcedure;


                cmd2.Parameters.Add("@CriteriaID", SqlDbType.NVarChar, 50);
                cmd2.Parameters.Add("@RubricID", SqlDbType.Int);
                cmd2.Parameters.Add("@FacetID", SqlDbType.NVarChar, 100);
                cmd2.Parameters.Add("@Content", SqlDbType.NVarChar, 255);
                cmd2.Parameters.Add("@Score", SqlDbType.Int);

                cmd2.Parameters["@CriteriaID"].Value = criteria.CriteriaID;
                cmd2.Parameters["@RubricID"].Value = newRubricID;
                cmd2.Parameters["@FacetID"].Value = rubric.FacetVMs[0].FacetID;
                cmd2.Parameters["@Content"].Value = criteria.Content;
                cmd2.Parameters["@Score"].Value = criteria.Score;

                try
                {
                    conn.Open();
                    rowsAffected = cmd2.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    conn.Close();
                }
            }


            return newRubricID;
        }

        public List<RubricVM> RetrieveActiveRubricsVMs()
        {
            List<RubricVM> rubrics = new List<RubricVM>();


            // connection
            var conn = DBConnection.GetConnection();

            // command text
            var cmdTxt = "sp_select_active_rubrics";

            // command
            var cmd = new SqlCommand(cmdTxt, conn);

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
                        RubricVM rubric = new RubricVM()
                        {
                            RubricID = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.GetString(2),
                            DateCreated = reader.GetDateTime(3),
                            DateUpdated = reader.GetDateTime(4),
                            ScoreTypeID = reader.GetString(5),
                            RubricCreator = new User()
                            {
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

            // list of facets

            // list of criteria



            return rubrics;
         
        }

        public Rubric SelectRubricByRubricDetials(string name, string description, string scoreType, string rubricCreator)
        {
            Rubric rubric = null;

            // connection
            var conn = DBConnection.GetConnection();

            string cmdTxt = "sp_select_rubric_by_name_description_score_type_id_rubric_creator";
            var cmd = new SqlCommand(cmdTxt, conn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 50);
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 100);
            cmd.Parameters.Add("@ScoreTypeID", SqlDbType.NVarChar, 50);
            cmd.Parameters.Add("@RubricCreator", SqlDbType.NVarChar, 50);

            cmd.Parameters["@Name"].Value = name;
            cmd.Parameters["@Description"].Value = description;
            cmd.Parameters["@ScoreTypeID"].Value = scoreType;
            cmd.Parameters["@RubricCreator"].Value = rubricCreator;

            try
            {
                conn.Open();
                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    int rowCount = 0;

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
                            RubricCreator = new User()
                            {
                                UserID = reader.GetString(6),
                                GivenName = reader.GetString(7),
                                FamilyName = reader.GetString(8),
                                Active = reader.GetBoolean(9),
                                Roles = new List<string>()
                            },
                            Active = reader.GetBoolean(10)

                        };

                        rowCount++;

                    }

                    if (rowCount > 1)
                    {
                        throw new ApplicationException("Problem retrieving a unique rubric");
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
                            Active = reader.GetBoolean(10),
                            NumberOfCriteria = reader.GetInt32(11)
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
            var cmdTxt = "sp_select_active_rubrics";

            // command
            var cmd = new SqlCommand(cmdTxt, conn);

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

        public int UpdateRubricByRubricID(int rubricID, string oldName, string newName, string oldDescription, string newDescription, string oldScoreType, string newScoreType)
        {
            int rowsAffected = 0;

            // connection
            var conn = DBConnection.GetConnection();

            string cmdTxt = "sp_update_rubric_by_rubric_id";
            var cmd = new SqlCommand(cmdTxt, conn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@RubricID", rubricID);
            cmd.Parameters.AddWithValue("@OldName", oldName);
            cmd.Parameters.AddWithValue("OldDescription", oldDescription);
            cmd.Parameters.AddWithValue("OldScoreTypeID", oldScoreType);

            cmd.Parameters.Add("@NewName", SqlDbType.NVarChar, 50);
            cmd.Parameters.Add("@NewDescription", SqlDbType.NVarChar, 100);
            cmd.Parameters.Add("@NewScoreTypeID", SqlDbType.NVarChar, 50);

            cmd.Parameters["@NewName"].Value = newName;
            cmd.Parameters["@NewDescription"].Value = newDescription;
            cmd.Parameters["@NewScoreTypeID"].Value = newScoreType;
            

            try
            {
                conn.Open();
                rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }
            return rowsAffected;

        }
    }
}
