Use OLMSDbb;

--borrow book
CREATE PROCEDURE sp_BorrowBook
    @UserId INT,
    @BookId INT,
    @BorrowedAt DATETIME,
    @DueDate DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    -- Prevent borrowing if already borrowed and not returned
    IF EXISTS (SELECT 1 FROM BorrowRecords WHERE UserId = @UserId AND BookId = @BookId AND IsReturned = 0)
    BEGIN
        RAISERROR('Book already borrowed by the user.', 16, 1);
        RETURN;
    END

    -- Check if copies are available
    DECLARE @Available INT = (SELECT AvailableCopies FROM Books WHERE BookId = @BookId);
    IF @Available < 1
    BEGIN
        RAISERROR('No copies available.', 16, 1);
        RETURN;
    END

    -- Insert borrow record
    INSERT INTO BorrowRecords (UserId, BookId, BorrowedAt, DueDate, IsReturned, IsOverdue)
    VALUES (@UserId, @BookId, @BorrowedAt, @DueDate, 0, 0);

    -- Decrement available copies
    UPDATE Books SET AvailableCopies = AvailableCopies - 1 WHERE BookId = @BookId;
END


--Return Book
CREATE PROCEDURE sp_ReturnBook
    @UserId INT,
    @BookId INT,
    @ReturnDate DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @BorrowRecordId INT, @DueDate DATETIME, @IsOverdue BIT;

    SELECT TOP 1 @BorrowRecordId = BorrowRecordId, @DueDate = DueDate
    FROM BorrowRecords
    WHERE UserId = @UserId AND BookId = @BookId AND IsReturned = 0;

    IF @BorrowRecordId IS NULL
    BEGIN
        RAISERROR('No active borrow record found.', 16, 1);
        RETURN;
    END

    -- Update borrow record
    SET @IsOverdue = CASE WHEN @ReturnDate > @DueDate THEN 1 ELSE 0 END;

    UPDATE BorrowRecords
    SET ReturnedAt = @ReturnDate, IsReturned = 1, IsOverdue = @IsOverdue
    WHERE BorrowRecordId = @BorrowRecordId;

    -- Increment available copies
    UPDATE Books SET AvailableCopies = AvailableCopies + 1 WHERE BookId = @BookId;

    -- If overdue, create a fine
    IF @IsOverdue = 1
    BEGIN
        DECLARE @DaysOverdue INT = DATEDIFF(DAY, @DueDate, @ReturnDate);
        DECLARE @FineAmount DECIMAL(10,2) = @DaysOverdue * 10;
        INSERT INTO Fines (BorrowRecordId, FineAmount, IsPaid)
        VALUES (@BorrowRecordId, @FineAmount, 0);
    END
END


--Generate report
CREATE PROCEDURE sp_GenerateReport
AS
BEGIN
    SELECT 
        (SELECT COUNT(*) FROM Books) AS TotalBooks,
        (SELECT COUNT(*) FROM BorrowRecords WHERE IsReturned = 0) AS BorrowedBooks,
        (SELECT COUNT(*) FROM BorrowRecords WHERE IsReturned = 0 AND DueDate < GETDATE()) AS OverdueBooks,
        (SELECT ISNULL(SUM(FineAmount), 0) FROM Fines WHERE IsPaid = 1) AS FinesCollected,
        (SELECT TOP 1 b.Title FROM Books b JOIN BorrowRecords br ON b.BookId = br.BookId GROUP BY b.Title ORDER BY COUNT(*) DESC) AS MostBorrowedBook
END


--checkover due books
CREATE PROCEDURE sp_CheckOverdueBooks
AS
BEGIN
    SELECT br.BorrowRecordId, br.UserId, br.BookId, br.DueDate, u.Email, b.Title
    FROM BorrowRecords br
    INNER JOIN Users u ON br.UserId = u.UserId
    INNER JOIN Books b ON br.BookId = b.BookId
    WHERE br.IsReturned = 0 AND br.DueDate < GETDATE()
END



--fine
CREATE PROCEDURE CalculateFine
    @BorrowRecordId INT,
    @FineAmount DECIMAL(10,2) OUTPUT
AS
BEGIN
    DECLARE @DueDate DATETIME, @ReturnedAt DATETIME, @FinePerDay DECIMAL(10,2) = 10.00;

    SELECT @DueDate = DueDate, @ReturnedAt = ReturnedAt
    FROM BorrowRecords
    WHERE BorrowRecordId = @BorrowRecordId;

    IF @ReturnedAt IS NULL OR @ReturnedAt <= @DueDate
        SET @FineAmount = 0;
    ELSE
        SET @FineAmount = DATEDIFF(DAY, @DueDate, @ReturnedAt) * @FinePerDay;
END




ALTER PROCEDURE sp_BorrowBook
    @UserId INT,
    @BookId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @BorrowedAt DATETIME = GETDATE();
    DECLARE @DueDate DATETIME = DATEADD(DAY, 14, @BorrowedAt); -- 2-week borrow

    -- Prevent borrowing if already borrowed and not returned
    IF EXISTS (SELECT 1 FROM BorrowRecords WHERE UserId = @UserId AND BookId = @BookId AND IsReturned = 0)
    BEGIN
        RAISERROR('Book already borrowed by the user.', 16, 1);
        RETURN;
    END

    -- Check if copies are available
    DECLARE @Available INT = (SELECT AvailableCopies FROM Books WHERE BookId = @BookId);
    IF @Available < 1
    BEGIN
        RAISERROR('No copies available.', 16, 1);
        RETURN;
    END

    -- Insert borrow record
    INSERT INTO BorrowRecords (UserId, BookId, BorrowedAt, DueDate, IsReturned, IsOverdue)
    VALUES (@UserId, @BookId, @BorrowedAt, @DueDate, 0, 0);

    -- Decrement available copies
    UPDATE Books SET AvailableCopies = AvailableCopies - 1 WHERE BookId = @BookId;
END










ALTER PROCEDURE sp_CheckOverdueBooks
AS
BEGIN
    SELECT 
        br.BorrowRecordId,
        br.UserId,
        br.BookId,
        br.BorrowedAt,
        br.DueDate,
        br.ReturnedAt,
        br.IsOverdue,       
        br.IsReturned,      
        u.Email,
        b.Title
    FROM BorrowRecords br
    INNER JOIN Users u ON br.UserId = u.UserId
    INNER JOIN Books b ON br.BookId = b.BookId
    WHERE br.IsReturned = 0 AND br.DueDate < GETDATE()
END













ALTER PROCEDURE sp_CheckOverdueBooks
AS
BEGIN
    SELECT 
        br.BorrowRecordId,
        br.UserId,
        br.BookId,
        br.DueDate,
        br.BorrowedAt,
        br.ReturnedAt,
        br.IsOverdue,
        br.IsReturned,

        -- Nested User
        u.UserId AS [User.UserId],
        u.FullName AS [User.FullName],
        u.Email AS [User.Email],

        -- Nested Book
        b.BookId AS [Book.BookId],
        b.Title AS [Book.Title]

    FROM BorrowRecords br
    INNER JOIN Users u ON br.UserId = u.UserId
    INNER JOIN Books b ON br.BookId = b.BookId
    WHERE br.IsReturned = 0 AND br.DueDate < GETDATE()
END
