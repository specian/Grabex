using System.Data;
using System.Data.SqlClient;

namespace Grabex;

public class SqlDb(string connectionString)
{
	private SqlConnection GetConnection()
	{
		return new SqlConnection(connectionString);
	}

	public async Task<DataTable> ExecuteQueryAsync(string query, SqlParameter[]? parameters = null)
	{
		using var connection = GetConnection();
		await connection.OpenAsync();
		using var command = new SqlCommand(query, connection);
		if (parameters != null)
		{
			command.Parameters.AddRange(parameters);
		}

		using var reader = await command.ExecuteReaderAsync();
		var dataTable = new DataTable();
		dataTable.Load(reader);
		return dataTable;
	}

	public async Task<int> ExecuteNonQueryAsync(string query, SqlParameter[]? parameters = null)
	{
		using var connection = GetConnection();
		await connection.OpenAsync();
		using var command = new SqlCommand(query, connection);
		if (parameters != null)
		{
			command.Parameters.AddRange(parameters);
		}

		return await command.ExecuteNonQueryAsync();
	}

	public async Task<object?> ExecuteScalarAsync(string query, SqlParameter[]? parameters = null)
	{
		using var connection = GetConnection();
		await connection.OpenAsync();
		using var command = new SqlCommand(query, connection);
		if (parameters != null)
		{
			command.Parameters.AddRange(parameters);
		}

		return await command.ExecuteScalarAsync();
	}

	public async Task<int> InsertAndGetIdAsync(string query, SqlParameter[]? parameters = null)
	{
		using var connection = GetConnection();
		await connection.OpenAsync();
		using var command = new SqlCommand(query + "; SELECT SCOPE_IDENTITY();", connection);
		if (parameters != null)
		{
			command.Parameters.AddRange(parameters);
		}

		object? result = await command.ExecuteScalarAsync();
		return Convert.ToInt32(result!);
	}
}
