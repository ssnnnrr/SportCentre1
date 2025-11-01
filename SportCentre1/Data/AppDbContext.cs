using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SportCentre1.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Client> Clients { get; set; }

    // Новая таблица для абонементов клиентов
    public virtual DbSet<ClientMembership> ClientMemberships { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Equipment> Equipment { get; set; }

    public virtual DbSet<Membershiptype> Membershiptypes { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Request> Requests { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<Trainer> Trainers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Workouttype> Workouttypes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=SportCentre;Username=postgres;Password=12345");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Bookingid).HasName("bookings_pkey");

            entity.ToTable("bookings");

            entity.Property(e => e.Bookingid).HasColumnName("bookingid");
            entity.Property(e => e.Bookingtime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("bookingtime");
            entity.Property(e => e.Clientid).HasColumnName("clientid");
            entity.Property(e => e.Ispaid)
                .HasDefaultValue(false)
                .HasColumnName("ispaid");
            entity.Property(e => e.Scheduleid).HasColumnName("scheduleid");

            entity.HasOne(d => d.Client).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.Clientid)
                .HasConstraintName("bookings_clientid_fkey");

            entity.HasOne(d => d.Schedule).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.Scheduleid)
                .HasConstraintName("bookings_scheduleid_fkey");
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Clientid).HasName("clients_pkey");

            entity.ToTable("clients");

            entity.HasIndex(e => e.Phonenumber, "clients_phonenumber_key").IsUnique();

            entity.HasIndex(e => e.Userid, "clients_userid_key").IsUnique();

            entity.Property(e => e.Clientid).HasColumnName("clientid");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Firstname)
                .HasMaxLength(50)
                .HasColumnName("firstname");
            entity.Property(e => e.Lastname)
                .HasMaxLength(50)
                .HasColumnName("lastname");
            entity.Property(e => e.Phonenumber)
                .HasMaxLength(20)
                .HasColumnName("phonenumber");
            entity.Property(e => e.Registrationdate).HasColumnName("registrationdate");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithOne(p => p.Client)
                .HasForeignKey<Client>(d => d.Userid)
                .HasConstraintName("clients_userid_fkey");
        });

        // Новая сущность для абонементов клиентов
        modelBuilder.Entity<ClientMembership>(entity =>
        {
            entity.HasKey(e => e.ClientMembershipId).HasName("clientmemberships_pkey");
            entity.ToTable("clientmemberships");
            entity.Property(e => e.ClientMembershipId).HasColumnName("clientmembershipid").UseIdentityByDefaultColumn();
            entity.Property(e => e.Clientid).HasColumnName("clientid");
            entity.Property(e => e.Membershiptypeid).HasColumnName("membershiptypeid");
            entity.Property(e => e.StartDate).HasColumnName("startdate");
            entity.Property(e => e.EndDate).HasColumnName("enddate");

            entity.HasOne(d => d.Client).WithMany(p => p.ClientMemberships)
                .HasForeignKey(d => d.Clientid)
                .HasConstraintName("clientmemberships_clientid_fkey");

            entity.HasOne(d => d.Membershiptype).WithMany()
                .HasForeignKey(d => d.Membershiptypeid)
                .HasConstraintName("clientmemberships_membershiptypeid_fkey");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Employeeid).HasName("employees_pkey");

            entity.ToTable("employees");

            entity.HasIndex(e => e.Userid, "employees_userid_key").IsUnique();

            entity.Property(e => e.Employeeid).HasColumnName("employeeid");
            entity.Property(e => e.Firstname)
                .HasMaxLength(50)
                .HasColumnName("firstname");
            entity.Property(e => e.Lastname)
                .HasMaxLength(50)
                .HasColumnName("lastname");
            entity.Property(e => e.Position)
                .HasMaxLength(100)
                .HasColumnName("position");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithOne(p => p.Employee)
                .HasForeignKey<Employee>(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("employees_userid_fkey");
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.Equipmentid).HasName("equipment_pkey");

            entity.ToTable("equipment");

            entity.Property(e => e.Equipmentid).HasColumnName("equipmentid");
            entity.Property(e => e.Lastmaintenancedate).HasColumnName("lastmaintenancedate");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
        });

        modelBuilder.Entity<Membershiptype>(entity =>
        {
            entity.HasKey(e => e.Membershiptypeid).HasName("membershiptypes_pkey");

            entity.ToTable("membershiptypes");

            entity.Property(e => e.Membershiptypeid).HasColumnName("membershiptypeid");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Durationdays).HasColumnName("durationdays");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.Typename)
                .HasMaxLength(100)
                .HasColumnName("typename");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Messageid).HasName("messages_pkey");

            entity.ToTable("messages");

            entity.Property(e => e.Messageid).HasColumnName("messageid");
            entity.Property(e => e.Messagetext).HasColumnName("messagetext");
            entity.Property(e => e.Requestid).HasColumnName("requestid");
            entity.Property(e => e.Senderuserid).HasColumnName("senderuserid");
            entity.Property(e => e.Sentdate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("sentdate");

            entity.HasOne(d => d.Request).WithMany(p => p.Messages)
                .HasForeignKey(d => d.Requestid)
                .HasConstraintName("messages_requestid_fkey");

            entity.HasOne(d => d.Senderuser).WithMany(p => p.Messages)
                .HasForeignKey(d => d.Senderuserid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("messages_senderuserid_fkey");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Paymentid).HasName("payments_pkey");

            entity.ToTable("payments");

            entity.Property(e => e.Paymentid).HasColumnName("paymentid");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.Bookingid).HasColumnName("bookingid");
            entity.Property(e => e.Clientid).HasColumnName("clientid");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Paymentdate).HasColumnName("paymentdate");

            // Добавляем необязательную связь с типом абонемента
            entity.Property(e => e.Membershiptypeid).HasColumnName("membershiptypeid");


            entity.HasOne(d => d.Booking).WithMany(p => p.Payments)
                .HasForeignKey(d => d.Bookingid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("payments_bookingid_fkey");

            entity.HasOne(d => d.Client).WithMany(p => p.Payments)
                .HasForeignKey(d => d.Clientid)
                .HasConstraintName("payments_clientid_fkey");

            // Новая связь для платежей за абонементы
            entity.HasOne<Membershiptype>()
                  .WithMany()
                  .HasForeignKey(d => d.Membershiptypeid)
                  .OnDelete(DeleteBehavior.SetNull)
                  .HasConstraintName("payments_membershiptypeid_fkey");
        });

        modelBuilder.Entity<Request>(entity =>
        {
            entity.HasKey(e => e.Requestid).HasName("requests_pkey");

            entity.ToTable("requests");

            entity.Property(e => e.Requestid).HasColumnName("requestid");
            entity.Property(e => e.Clientid).HasColumnName("clientid");
            entity.Property(e => e.Creationdate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("creationdate");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.Subject)
                .HasMaxLength(255)
                .HasColumnName("subject");

            entity.HasOne(d => d.Client).WithMany(p => p.Requests)
                .HasForeignKey(d => d.Clientid)
                .HasConstraintName("requests_clientid_fkey");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Reviewid).HasName("reviews_pkey");

            entity.ToTable("reviews");

            entity.Property(e => e.Reviewid).HasColumnName("reviewid");
            entity.Property(e => e.Clientid).HasColumnName("clientid");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Reviewdate).HasColumnName("reviewdate");
            entity.Property(e => e.Trainerid).HasColumnName("trainerid");

            entity.HasOne(d => d.Client).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.Clientid)
                .HasConstraintName("reviews_clientid_fkey");

            entity.HasOne(d => d.Trainer).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.Trainerid)
                .HasConstraintName("reviews_trainerid_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Roleid).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Rolename, "roles_rolename_key").IsUnique();

            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Rolename)
                .HasMaxLength(50)
                .HasColumnName("rolename");
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.Scheduleid).HasName("schedule_pkey");

            entity.ToTable("schedule");

            entity.Property(e => e.Scheduleid).HasColumnName("scheduleid");
            entity.Property(e => e.Currentenrollment)
                .HasDefaultValue(0)
                .HasColumnName("currentenrollment");
            entity.Property(e => e.Endtime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("endtime");
            entity.Property(e => e.Maxcapacity).HasColumnName("maxcapacity");
            entity.Property(e => e.Starttime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("starttime");
            entity.Property(e => e.Trainerid).HasColumnName("trainerid");
            entity.Property(e => e.Workouttypeid).HasColumnName("workouttypeid");

            entity.HasOne(d => d.Trainer).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.Trainerid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("schedule_trainerid_fkey");

            entity.HasOne(d => d.Workouttype).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.Workouttypeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("schedule_workouttypeid_fkey");
        });

        modelBuilder.Entity<Trainer>(entity =>
        {
            entity.HasKey(e => e.Trainerid).HasName("trainers_pkey");

            entity.ToTable("trainers");

            entity.HasIndex(e => e.Userid, "trainers_userid_key").IsUnique();

            entity.Property(e => e.Trainerid).HasColumnName("trainerid");
            entity.Property(e => e.Firstname)
                .HasMaxLength(50)
                .HasColumnName("firstname");
            entity.Property(e => e.Lastname)
                .HasMaxLength(50)
                .HasColumnName("lastname");
            entity.Property(e => e.Specialization)
                .HasMaxLength(100)
                .HasColumnName("specialization");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithOne(p => p.Trainer)
                .HasForeignKey<Trainer>(d => d.Userid)
                .HasConstraintName("trainers_userid_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .HasColumnName("password");
            entity.Property(e => e.Roleid).HasColumnName("roleid");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.Roleid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("users_roleid_fkey");
        });

        modelBuilder.Entity<Workouttype>(entity =>
        {
            entity.HasKey(e => e.Workouttypeid).HasName("workouttypes_pkey");

            entity.ToTable("workouttypes");

            entity.Property(e => e.Workouttypeid).HasColumnName("workouttypeid");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("500.00")
                .HasColumnName("price");
            entity.Property(e => e.Typename)
                .HasMaxLength(100)
                .HasColumnName("typename");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}