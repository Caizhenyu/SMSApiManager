using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SMSApiManager.Migrations
{
    public partial class InititalKey4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Member_AspNetUsers_OwnerId",
                table: "Member");

            migrationBuilder.DropForeignKey(
                name: "FK_Record_AspNetUsers_OwnerId",
                table: "Record");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "Record",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "Member",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Member_AspNetUsers_OwnerId",
                table: "Member",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Record_AspNetUsers_OwnerId",
                table: "Record",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Member_AspNetUsers_OwnerId",
                table: "Member");

            migrationBuilder.DropForeignKey(
                name: "FK_Record_AspNetUsers_OwnerId",
                table: "Record");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "Record",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "Member",
                nullable: true,
                oldClrType: typeof(string));

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
    }
}
