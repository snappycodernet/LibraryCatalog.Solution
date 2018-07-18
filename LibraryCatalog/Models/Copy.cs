﻿using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace LibraryCatalog.Models
{
    public class Copy
    {
        public int Id { get; set; }
        private bool checkedOut { get; set; }
        public DateTime DueDate { get; set; }

        public Copy(DateTime dueDate, int id = 0)
        {

            Id = id;
            DueDate = dueDate;
            checkedOut = true;
        }

        public override int GetHashCode()
        {
            return this.checkedOut.GetHashCode();
        }

        public override bool Equals(System.Object otherCopy)
        {
            if (!(otherCopy is Copy))
            {
                return false;
            }
            else
            {
                Copy newCopy = (Copy)otherCopy;
                bool idEquality = (this.Id == newCopy.Id);

                return (idEquality);
            }
        }



        public void Save()
        {
            MySqlConnection conn = DB.Connection();
            conn.Open();

            var cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"INSERT INTO copies (dueDate) VALUES (@dueDate);";

            MySqlParameter dueDate = new MySqlParameter();
            dueDate.ParameterName = "@dueDate";
            dueDate.Value = DueDate;
            cmd.Parameters.Add(dueDate);

            cmd.ExecuteNonQuery();
            Id = (int)cmd.LastInsertedId;

            conn.Close();
            if (conn != null)
            {
                conn.Dispose();
            }
        }

        public static List<Copy> GetAll()
        {
            List<Copy> allCopies = new List<Copy> { };
            MySqlConnection conn = DB.Connection();
            conn.Open();

            MySqlCommand cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"SELECT * FROM copies;";

            MySqlDataReader rdr = cmd.ExecuteReader() as MySqlDataReader;

            while (rdr.Read())
            {
                int id = rdr.GetInt32(0);
                DateTime dueDate = rdr.GetDateTime(1);

                Copy newCopy = new Copy(dueDate, id);
                allCopies.Add(newCopy);
            }

            conn.Close();

            if (conn != null)
            {
                conn.Dispose();
            }

            return allCopies;
        }

        public static Copy Find(int id)
        {
            MySqlConnection conn = DB.Connection();
            conn.Open();

            var cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"SELECT * FROM copies WHERE id = @thisId;";

            MySqlParameter thisId = new MySqlParameter();
            thisId.ParameterName = "@thisId";
            thisId.Value = id;
            cmd.Parameters.Add(thisId);

            var rdr = cmd.ExecuteReader() as MySqlDataReader;

            int copyId = 0;
            DateTime dueDate = rdr.GetDateTime(1);

            while (rdr.Read())
            {
                copyId = rdr.GetInt32(0);
                dueDate = rdr.GetDateTime(1);
            }

            Copy foundCopy = new Copy(dueDate, copyId);

            conn.Close();
            if (conn != null)
            {
                conn.Dispose();
            }

            return foundCopy;
        }

        public void Delete()
        {
            MySqlConnection conn = DB.Connection();
            conn.Open();
            var cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"DELETE FROM copies WHERE id = @CopyId; DELETE FROM checkout WHERE copies_id = @CopyId;";

            MySqlParameter copyIdParameter = new MySqlParameter();
            copyIdParameter.ParameterName = "@CopyId";
            copyIdParameter.Value = Id;
            cmd.Parameters.Add(copyIdParameter);

            cmd.ExecuteNonQuery();
            if (conn != null)
            {
                conn.Close();
            }
        }

        public static void DeleteAll()
        {
            MySqlConnection conn = DB.Connection();
            conn.Open();

            var cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"DELETE FROM copies;";

            cmd.ExecuteNonQuery();

            conn.Close();
            if (conn != null)
            {
                conn.Dispose();
            }
        }

        public void AddPatron(Patron newPatron)
        {
            MySqlConnection conn = DB.Connection();
            conn.Open();
            var cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"INSERT INTO checkout (copy_id, patron_id) VALUES (@CopyId, @PatronId);";

            MySqlParameter copy_id = new MySqlParameter();
            copy_id.ParameterName = "@CopyId";
            copy_id.Value = Id;
            cmd.Parameters.Add(copy_id);

            MySqlParameter author_id = new MySqlParameter();
            author_id.ParameterName = "@PatronId";
            author_id.Value = newPatron.Id;
            cmd.Parameters.Add(author_id);

            cmd.ExecuteNonQuery();
            conn.Close();
            if (conn != null)
            {
                conn.Dispose();
            }
        }

        public Patron GetPatron()
        {
            MySqlConnection conn = DB.Connection();
            conn.Open();
            var cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"SELECT patrons.* FROM copies
                                JOIN checkout ON (copies.id = checkout.copy_id)
                                JOIN patrons ON (checkout.patron_id = patrons.id)
                                WHERE copies.id = @copyId;";

            MySqlParameter copyIdParameter = new MySqlParameter();
            copyIdParameter.ParameterName = "@copyId";
            copyIdParameter.Value = Id;
            cmd.Parameters.Add(copyIdParameter);

            var rdr = cmd.ExecuteReader() as MySqlDataReader;

            Patron copyPatron = null;

            while (rdr.Read())
            {
                int patronId = rdr.GetInt32(0);
                string name = rdr.GetString(1);
                copyPatron = new Patron(name, patronId);
            }

            conn.Close();
            if (conn != null)
            {
                conn.Dispose();
            }

            return copyPatron;
        }

        public List<Patron> GetAllPatrons()
        {
            return Patron.GetAll();
        }
    }
}