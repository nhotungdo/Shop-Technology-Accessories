-- Script kiểm tra dữ liệu OrderDetails
USE ShopTechnologyAccessories;
GO

-- Kiểm tra tổng số OrderDetails
SELECT 'Total OrderDetails' as Info, COUNT(*) as Count FROM OrderDetails;
GO

-- Kiểm tra OrderDetails theo OrderId
SELECT 'OrderDetails by OrderId' as Info, OrderId, COUNT(*) as Count 
FROM OrderDetails 
GROUP BY OrderId 
ORDER BY OrderId;
GO

-- Kiểm tra chi tiết OrderDetails
SELECT TOP 10 
    od.OrderDetailId,
    od.OrderId,
    od.ProductId,
    od.ProductName,
    od.Quantity,
    od.UnitPrice,
    od.TotalPrice,
    o.OrderNumber,
    o.CustomerName
FROM OrderDetails od
LEFT JOIN Orders o ON od.OrderId = o.OrderId
ORDER BY od.OrderDetailId DESC;
GO

-- Kiểm tra Orders
SELECT 'Total Orders' as Info, COUNT(*) as Count FROM Orders;
GO

-- Kiểm tra Orders gần đây
SELECT TOP 5 
    OrderId,
    OrderNumber,
    CustomerName,
    TotalAmount,
    OrderStatus,
    CreatedAt
FROM Orders 
ORDER BY OrderId DESC;
GO
