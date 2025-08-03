using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagement.Web.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLoaiNguoiDung_NHANVIEN_to_ADMIN : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ThanhToans_DangKys_DangKyId",
                table: "ThanhToans");

            migrationBuilder.AlterColumn<int>(
                name: "DangKyId",
                table: "ThanhToans",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "LoaiDangKy",
                table: "LopHocs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateOnly>(
                name: "NgayBatDauKhoa",
                table: "LopHocs",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "NgayKetThucKhoa",
                table: "LopHocs",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoaiDangKy",
                table: "DangKys",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TrangThaiChiTiet",
                table: "DangKys",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "KichHoat",
                table: "CauHinhHoaHongs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LopHocId",
                table: "CauHinhHoaHongs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MoTa",
                table: "CauHinhHoaHongs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayTao",
                table: "CauHinhHoaHongs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayTao",
                table: "BangLuongs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhHoaHongs_LopHocId",
                table: "CauHinhHoaHongs",
                column: "LopHocId");

            migrationBuilder.AddForeignKey(
                name: "FK_CauHinhHoaHongs_LopHocs_LopHocId",
                table: "CauHinhHoaHongs",
                column: "LopHocId",
                principalTable: "LopHocs",
                principalColumn: "LopHocId");

            migrationBuilder.AddForeignKey(
                name: "FK_ThanhToans_DangKys_DangKyId",
                table: "ThanhToans",
                column: "DangKyId",
                principalTable: "DangKys",
                principalColumn: "DangKyId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CauHinhHoaHongs_LopHocs_LopHocId",
                table: "CauHinhHoaHongs");

            migrationBuilder.DropForeignKey(
                name: "FK_ThanhToans_DangKys_DangKyId",
                table: "ThanhToans");

            migrationBuilder.DropIndex(
                name: "IX_CauHinhHoaHongs_LopHocId",
                table: "CauHinhHoaHongs");

            migrationBuilder.DropColumn(
                name: "LoaiDangKy",
                table: "LopHocs");

            migrationBuilder.DropColumn(
                name: "NgayBatDauKhoa",
                table: "LopHocs");

            migrationBuilder.DropColumn(
                name: "NgayKetThucKhoa",
                table: "LopHocs");

            migrationBuilder.DropColumn(
                name: "LoaiDangKy",
                table: "DangKys");

            migrationBuilder.DropColumn(
                name: "TrangThaiChiTiet",
                table: "DangKys");

            migrationBuilder.DropColumn(
                name: "KichHoat",
                table: "CauHinhHoaHongs");

            migrationBuilder.DropColumn(
                name: "LopHocId",
                table: "CauHinhHoaHongs");

            migrationBuilder.DropColumn(
                name: "MoTa",
                table: "CauHinhHoaHongs");

            migrationBuilder.DropColumn(
                name: "NgayTao",
                table: "CauHinhHoaHongs");

            migrationBuilder.DropColumn(
                name: "NgayTao",
                table: "BangLuongs");

            migrationBuilder.AlterColumn<int>(
                name: "DangKyId",
                table: "ThanhToans",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ThanhToans_DangKys_DangKyId",
                table: "ThanhToans",
                column: "DangKyId",
                principalTable: "DangKys",
                principalColumn: "DangKyId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
