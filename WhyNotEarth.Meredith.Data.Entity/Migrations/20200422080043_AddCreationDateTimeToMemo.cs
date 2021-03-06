﻿using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace WhyNotEarth.Meredith.Data.Entity.Migrations
{
    public partial class AddCreationDateTimeToMemo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDateTime",
                schema: "ModuleVolkswagen",
                table: "Memos",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreationDateTime",
                schema: "ModuleVolkswagen",
                table: "Memos");
        }
    }
}
