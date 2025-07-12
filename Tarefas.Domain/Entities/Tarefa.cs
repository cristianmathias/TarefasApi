namespace Tarefas.Domain.Entities;

public class Tarefa
{
    public int Id { get; set; }
    public required string Titulo { get; set; }
    public required string Descricao { get; set; }
    public bool Concluida { get; set; }
}