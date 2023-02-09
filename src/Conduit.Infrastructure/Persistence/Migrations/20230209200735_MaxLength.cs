using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conduit.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class MaxLength : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Password",
            table: "Users",
            type: "character varying(255)",
            maxLength: 255,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "varchar(255)",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "Users",
            type: "character varying(255)",
            maxLength: 255,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(255)");

        migrationBuilder.AlterColumn<string>(
            name: "Image",
            table: "Users",
            type: "character varying(255)",
            maxLength: 255,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "varchar(255)",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Email",
            table: "Users",
            type: "character varying(255)",
            maxLength: 255,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(255)");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "Tags",
            type: "character varying(255)",
            maxLength: 255,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(255)");

        migrationBuilder.AlterColumn<string>(
            name: "Title",
            table: "Articles",
            type: "character varying(255)",
            maxLength: 255,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(255)");

        migrationBuilder.AlterColumn<string>(
            name: "Slug",
            table: "Articles",
            type: "character varying(255)",
            maxLength: 255,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(255)");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Password",
            table: "Users",
            type: "varchar(255)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(255)",
            oldMaxLength: 255,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "Users",
            type: "varchar(255)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(255)",
            oldMaxLength: 255);

        migrationBuilder.AlterColumn<string>(
            name: "Image",
            table: "Users",
            type: "varchar(255)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(255)",
            oldMaxLength: 255,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Email",
            table: "Users",
            type: "varchar(255)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(255)",
            oldMaxLength: 255);

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "Tags",
            type: "varchar(255)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(255)",
            oldMaxLength: 255);

        migrationBuilder.AlterColumn<string>(
            name: "Title",
            table: "Articles",
            type: "varchar(255)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(255)",
            oldMaxLength: 255);

        migrationBuilder.AlterColumn<string>(
            name: "Slug",
            table: "Articles",
            type: "varchar(255)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(255)",
            oldMaxLength: 255);
    }
}