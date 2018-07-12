using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SMSApiManager.Migrations
{
    public partial class InitialKey1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Api",
                newName: "ApiId");

            migrationBuilder.CreateIndex(
                name: "IX_Record_ApiId",
                table: "Record",
                column: "ApiId");

            migrationBuilder.AddForeignKey(
                name: "FK_Record_Api_ApiId",
                table: "Record",
                column: "ApiId",
                principalTable: "Api",
                principalColumn: "ApiId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Record_Api_ApiId",
                table: "Record");

            migrationBuilder.DropIndex(
                name: "IX_Record_ApiId",
                table: "Record");

            migrationBuilder.RenameColumn(
                name: "ApiId",
                table: "Api",
                newName: "Id");
        }
    }
}
