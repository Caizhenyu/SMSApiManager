using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SMSApiManager.Migrations
{
    public partial class InitialKey2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AppliactionUserID",
                table: "Member",
                newName: "OwnerId");

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Record",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "Member",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Record_OwnerId",
                table: "Record",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Member_OwnerId",
                table: "Member",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Member_AspNetUsers_OwnerId",
                table: "Member",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Record_AspNetUsers_OwnerId",
                table: "Record",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Member_AspNetUsers_OwnerId",
                table: "Member");

            migrationBuilder.DropForeignKey(
                name: "FK_Record_AspNetUsers_OwnerId",
                table: "Record");

            migrationBuilder.DropIndex(
                name: "IX_Record_OwnerId",
                table: "Record");

            migrationBuilder.DropIndex(
                name: "IX_Member_OwnerId",
                table: "Member");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Record");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Member",
                newName: "AppliactionUserID");

            migrationBuilder.AlterColumn<string>(
                name: "AppliactionUserID",
                table: "Member",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
