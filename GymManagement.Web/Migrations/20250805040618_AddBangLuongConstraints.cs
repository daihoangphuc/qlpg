using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagement.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddBangLuongConstraints : Migration
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

            migrationBuilder.AlterColumn<decimal>(
                name: "PhanTramHoaHong",
                table: "CauHinhHoaHongs",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

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

            migrationBuilder.CreateTable(
                name: "TinTucs",
                columns: table => new
                {
                    TinTucId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TieuDe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoTaNgan = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnhDaiDien = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayXuatBan = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TacGiaId = table.Column<int>(type: "int", nullable: true),
                    TenTacGia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LuotXem = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "DRAFT"),
                    NoiBat = table.Column<bool>(type: "bit", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MetaTitle = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    MetaDescription = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    MetaKeywords = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinTucs", x => x.TinTucId);
                    table.ForeignKey(
                        name: "FK_TinTucs_NguoiDungs_TacGiaId",
                        column: x => x.TacGiaId,
                        principalTable: "NguoiDungs",
                        principalColumn: "NguoiDungId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TinTucs_Slug",
                table: "TinTucs",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TinTucs_TacGiaId",
                table: "TinTucs",
                column: "TacGiaId");

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
                name: "FK_ThanhToans_DangKys_DangKyId",
                table: "ThanhToans");

            migrationBuilder.DropTable(
                name: "TinTucs");

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

            migrationBuilder.AlterColumn<int>(
                name: "PhanTramHoaHong",
                table: "CauHinhHoaHongs",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

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
