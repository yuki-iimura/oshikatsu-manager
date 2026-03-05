using Microsoft.Data.Sqlite;

var connection = new SqliteConnection("Data Source=oshikatsu.db");
connection.Open();

// テーブル作成
var createTable = connection.CreateCommand();
createTable.CommandText = @"
    CREATE TABLE IF NOT EXISTS Lives (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        Date TEXT NOT NULL,
        Venue TEXT NOT NULL,
        Idol TEXT NOT NULL,
        Cost INTEGER NOT NULL,
        Memo TEXT
    )";
createTable.ExecuteNonQuery();

// メインループ
while (true)
{
    Console.WriteLine("\n===== 推し活マネージャー =====");
    Console.WriteLine("1. ライブを登録する");
    Console.WriteLine("2. 一覧を見る");
    Console.WriteLine("3. ライブを削除する");
    Console.WriteLine("4. 合計金額を見る");
    Console.WriteLine("5. 終了");
    Console.Write("番号を入力: ");

    var input = Console.ReadLine();

    if (input == "1") AddLive(connection);
    else if (input == "2") ShowLives(connection);
    else if (input == "3") DeleteLive(connection);
    else if (input == "4") ShowTotal(connection);
    else if (input == "5") break;
    else Console.WriteLine("1〜5で入力してください");
}

connection.Close();
Console.WriteLine("終了しました");

// ライブ登録
static void AddLive(SqliteConnection conn)
{
    Console.Write("日付（例：2025-04-01）: ");
    var date = Console.ReadLine();

    Console.Write("会場名: ");
    var venue = Console.ReadLine();

    Console.Write("アイドル名: ");
    var idol = Console.ReadLine();

    Console.Write("使った金額（円）: ");
    var cost = Console.ReadLine();

    Console.Write("メモ（任意、なければEnter）: ");
    var memo = Console.ReadLine();

    var cmd = conn.CreateCommand();
    cmd.CommandText = @"
        INSERT INTO Lives (Date, Venue, Idol, Cost, Memo)
        VALUES ($date, $venue, $idol, $cost, $memo)";
    cmd.Parameters.AddWithValue("$date", date);
    cmd.Parameters.AddWithValue("$venue", venue);
    cmd.Parameters.AddWithValue("$idol", idol);
    cmd.Parameters.AddWithValue("$cost", cost ?? "0");
    cmd.Parameters.AddWithValue("$memo", memo ?? "");
    cmd.ExecuteNonQuery();

    Console.WriteLine("登録しました！");
}

// 一覧表示
static void ShowLives(SqliteConnection conn)
{
    var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT * FROM Lives ORDER BY Date DESC";
    var reader = cmd.ExecuteReader();

    Console.WriteLine("\n--- ライブ履歴 ---");
    bool hasData = false;
    while (reader.Read())
    {
        hasData = true;
        Console.WriteLine($"[{reader["Id"]}] {reader["Date"]} | {reader["Idol"]} | {reader["Venue"]} | {reader["Cost"]}円 | {reader["Memo"]}");
    }

    if (!hasData) Console.WriteLine("まだ登録がありません");
}

// 削除
static void DeleteLive(SqliteConnection conn)
{
    // まず一覧を表示
    var showCmd = conn.CreateCommand();
    showCmd.CommandText = "SELECT * FROM Lives ORDER BY Date DESC";
    var reader = showCmd.ExecuteReader();

    Console.WriteLine("\n--- ライブ履歴 ---");
    bool hasData = false;
    while (reader.Read())
    {
        hasData = true;
        Console.WriteLine($"[{reader["Id"]}] {reader["Date"]} | {reader["Idol"]} | {reader["Venue"]} | {reader["Cost"]}円 | {reader["Memo"]}");
    }

    if (!hasData)
    {
        Console.WriteLine("まだ登録がありません");
        return;
    }

    Console.Write("\n削除するIDを入力: ");
    var idInput = Console.ReadLine();

    if (!int.TryParse(idInput, out int id))
    {
        Console.WriteLine("正しいIDを入力してください");
        return;
    }

    Console.Write($"ID:{id} を削除しますか？（y/n）: ");
    var confirm = Console.ReadLine();
    if (confirm?.ToLower() != "y")
    {
        Console.WriteLine("キャンセルしました");
        return;
    }

    var deleteCmd = conn.CreateCommand();
    deleteCmd.CommandText = "DELETE FROM Lives WHERE Id = $id";
    deleteCmd.Parameters.AddWithValue("$id", id);
    int rows = deleteCmd.ExecuteNonQuery();

    Console.WriteLine(rows > 0 ? "削除しました！" : "該当するIDが見つかりませんでした");
}

// 合計金額
static void ShowTotal(SqliteConnection conn)
{
    var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT Idol, SUM(Cost) as Total, COUNT(*) as Count FROM Lives GROUP BY Idol ORDER BY Total DESC";
    var reader = cmd.ExecuteReader();

    Console.WriteLine("\n--- 推し別合計 ---");
    bool hasData = false;
    int grandTotal = 0;

    while (reader.Read())
    {
        hasData = true;
        int total = Convert.ToInt32(reader["Total"]);
        grandTotal += total;
        Console.WriteLine($"{reader["Idol"]} | {reader["Count"]}回 | 合計 {total:N0}円");
    }

    if (!hasData)
    {
        Console.WriteLine("まだ登録がありません");
        return;
    }

    Console.WriteLine($"\n💰 総合計: {grandTotal:N0}円");
}