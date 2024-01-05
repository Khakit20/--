using Microsoft.EntityFrameworkCore.Migrations;

namespace project_manage_system_backend.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Jiras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DomainURL = table.Column<string>(type: "TEXT", nullable: true),
                    APIToken = table.Column<string>(type: "TEXT", nullable: true),
                    Account = table.Column<string>(type: "TEXT", nullable: true),
                    BoardId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jiras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Account = table.Column<string>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    AvatarUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Authority = table.Column<string>(type: "TEXT", nullable: true),
                    TrelloKey = table.Column<string>(type: "TEXT", nullable: true),
                    TrelloToken = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Account);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    OwnerAccount = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Projects_Users_OwnerAccount",
                        column: x => x.OwnerAccount,
                        principalTable: "Users",
                        principalColumn: "Account",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Invitations",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InviterAccount = table.Column<string>(type: "TEXT", nullable: true),
                    ApplicantAccount = table.Column<string>(type: "TEXT", nullable: true),
                    InvitedProjectID = table.Column<int>(type: "INTEGER", nullable: true),
                    IsAgreed = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invitations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Invitations_Projects_InvitedProjectID",
                        column: x => x.InvitedProjectID,
                        principalTable: "Projects",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invitations_Users_ApplicantAccount",
                        column: x => x.ApplicantAccount,
                        principalTable: "Users",
                        principalColumn: "Account",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invitations_Users_InviterAccount",
                        column: x => x.InviterAccount,
                        principalTable: "Users",
                        principalColumn: "Account",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Repositories",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Url = table.Column<string>(type: "TEXT", nullable: true),
                    Owner = table.Column<string>(type: "TEXT", nullable: true),
                    ProjectID = table.Column<int>(type: "INTEGER", nullable: true),
                    IsSonarqube = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccountColonPw = table.Column<string>(type: "TEXT", nullable: true),
                    SonarqubeUrl = table.Column<string>(type: "TEXT", nullable: true),
                    ProjectKey = table.Column<string>(type: "TEXT", nullable: true),
                    RepoId = table.Column<string>(type: "TEXT", nullable: true),
                    BoardId = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repositories", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Repositories_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "Projects",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserProject",
                columns: table => new
                {
                    Account = table.Column<string>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProject", x => new { x.ProjectId, x.Account });
                    table.ForeignKey(
                        name: "FK_UserProject_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserProject_Users_Account",
                        column: x => x.Account,
                        principalTable: "Users",
                        principalColumn: "Account",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_ApplicantAccount",
                table: "Invitations",
                column: "ApplicantAccount");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_InvitedProjectID",
                table: "Invitations",
                column: "InvitedProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_InviterAccount",
                table: "Invitations",
                column: "InviterAccount");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_OwnerAccount",
                table: "Projects",
                column: "OwnerAccount");

            migrationBuilder.CreateIndex(
                name: "IX_Repositories_ProjectID",
                table: "Repositories",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_UserProject_Account",
                table: "UserProject",
                column: "Account");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Invitations");

            migrationBuilder.DropTable(
                name: "Jiras");

            migrationBuilder.DropTable(
                name: "Repositories");

            migrationBuilder.DropTable(
                name: "UserProject");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
