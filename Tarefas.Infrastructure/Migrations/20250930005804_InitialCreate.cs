using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tarefas.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tarefas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, comment: "Título da tarefa"),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false, comment: "Descrição detalhada da tarefa"),
                    Concluida = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false, comment: "Indica se a tarefa foi concluída"),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')", comment: "Data de criação da tarefa"),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')", comment: "Data da última atualização"),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false, comment: "Soft delete flag"),
                    DataExclusao = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Data da exclusão lógica")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tarefas", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Tarefas",
                columns: new[] { "Id", "Concluida", "DataAtualizacao", "DataCriacao", "DataExclusao", "Descricao", "Titulo" },
                values: new object[] { 1, true, new DateTime(2025, 9, 29, 0, 58, 2, 962, DateTimeKind.Utc).AddTicks(9486), new DateTime(2025, 9, 23, 0, 58, 2, 962, DateTimeKind.Utc).AddTicks(8622), null, "Criar endpoints para CRUD de tarefas usando Clean Architecture", "Implementar API REST" });

            migrationBuilder.InsertData(
                table: "Tarefas",
                columns: new[] { "Id", "DataAtualizacao", "DataCriacao", "DataExclusao", "Descricao", "Titulo" },
                values: new object[] { 2, new DateTime(2025, 9, 27, 0, 58, 2, 963, DateTimeKind.Utc).AddTicks(876), new DateTime(2025, 9, 27, 0, 58, 2, 963, DateTimeKind.Utc).AddTicks(873), null, "Adicionar EF Core com SQLite para persistência de dados", "Configurar Entity Framework" });

            migrationBuilder.InsertData(
                table: "Tarefas",
                columns: new[] { "Id", "Concluida", "DataAtualizacao", "DataCriacao", "DataExclusao", "Descricao", "Titulo" },
                values: new object[] { 3, true, new DateTime(2025, 9, 29, 22, 58, 2, 963, DateTimeKind.Utc).AddTicks(942), new DateTime(2025, 9, 25, 0, 58, 2, 963, DateTimeKind.Utc).AddTicks(940), null, "Criar testes unitários e de integração abrangentes", "Implementar Testes" });

            migrationBuilder.CreateIndex(
                name: "IX_Tarefas_Concluida",
                table: "Tarefas",
                column: "Concluida");

            migrationBuilder.CreateIndex(
                name: "IX_Tarefas_Concluida_IsDeleted",
                table: "Tarefas",
                columns: new[] { "Concluida", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Tarefas_DataCriacao",
                table: "Tarefas",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_Tarefas_IsDeleted",
                table: "Tarefas",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tarefas");
        }
    }
}
