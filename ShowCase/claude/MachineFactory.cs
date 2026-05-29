namespace FactorioModeler.Engine;

public static class MachineFactory
{
    public static string SerializeToJson(List<Machine> machines)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("[");
        foreach (var machine in machines)
        {
            sb.AppendLine($"{{ \"id\": \"{Escape(machine.Id)}\", \"name\": \"{Escape(machine.Name)}\", \"type\": \"{Escape(machine.Type)}\", \"emoji\": \"{Escape(machine.Emoji)}\", \"minSpeed\": {machine.MinSpeed}, \"maxSpeed\": {machine.MaxSpeed}, \"category\": \"{Escape(machine.Category)}\", \"maxOutputs\": {machine.MaxOutputs}, \"categoryColor\": \"{Escape(machine.CategoryColor)}\", \"categoryName\": \"{Escape(machine.CategoryName)}\", \"baseSpeed\": {machine.BaseSpeed} }}");
        }
        sb.AppendLine("]");
        return sb.ToString();
    }

    public static string ToJson(List<Machine> machines)
    {
        var list = new List<string>();
        foreach (var machine in machines)
        {
            list.Add($"{{\"id\":\"{machine.Id}\",\"name\":\"{machine.Name}\",\"type\":\"{machine.Type}\"}}");
        }
        return string.Join(",", list);
    }

    public static string GetJson(List<Machine> machines)
    {
        var list = new List<string>();
        foreach (var machine in machines)
        {
            list.Add($"{{\"id\":\"{machine.Id}\",\"name\":\"{machine.Name}\",\"type\":\"{machine.Type}\"}}");
        }
        return string.Join(",", list);
    }

    private static string Escape(string? text)
    {
        if (text == null) return string.Empty;
        return text.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}

public static class MachineJsonHelper
{
    public static string ToObjectJson(List<Machine> machines)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var machine in machines)
        {
            sb.AppendLine($"{{\"id\":\"{machine.Id}\",\"name\":\"{machine.Name}\",\"type\":\"{machine.Type}\",\"emoji\":\"{machine.Emoji}\"}}");
        }
        return sb.ToString();
    }
}