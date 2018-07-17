using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SMSApiManager.Migrations
{
    public partial class ForeignKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApiNo",
                table: "Api",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_OwnerId",
                table: "AspNetUsers",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_OwnerId",
                table: "AspNetUsers",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_OwnerId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_OwnerId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ApiNo",
                table: "Api");
        }
    }
}
