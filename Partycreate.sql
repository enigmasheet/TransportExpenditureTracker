-- Delete all existing data
DELETE FROM [dbo].[Parties];

-- Insert cleaned data
INSERT INTO [dbo].[Parties] ([VatNo], [PartyName], [Location])
VALUES
('601585719', 'Aarati Oil Suppliers', 'Change Later'),
('603292530', 'Ananda Automobiles', 'Birgunj'),
('603069404', 'Anushka Impex', 'Birgunj'),
('300889763', 'Arman Automobiles', 'Birgunj'),
('305315313', 'Ashoka Paints And Chemical Indutries', 'Birgunj'),
('603568088', 'Asia Fuel Center Pvt Ltd', 'Budiganga'),
('301203991', 'Axn Trading Pvt Ltd', 'Birgunj'),
('302154539', 'Baishno Oil Stores', 'Narsingh'),
('607981328', 'Balaji Mobile Center', 'Birgunj'),
('605851201', 'Bhagat Traders', 'Rupani'),
('300276914', 'Bol Bom Petrol Pump', 'Birgunj'),
('609258833', 'Dhanusha Oil Stores', 'Birendrabazar'),
('300035058', 'Dinbandhu Oil And Trading House', 'Birgunj'),
('603511503', 'Everest Lubricants Pvt Ltd', 'Birgunj'),
('301729273', 'J.P. Store Motor Parts', 'Sapatari'),
('604821061', 'Jagat Oil Stores', 'Chandrapur'),
('303931078', 'Jagdamba Oil Distributor', 'Birgunj'),
('300213380', 'Jay Shree Tirupaati Automobiles Pvt Ltd', 'Biratnagar'),
('604298467', 'Kk Impex', 'Birgunj'),
('300096512', 'Mahendra Oil And Trade Centre', 'Itahari'),
('301972932', 'Messers Kishor And Sons', 'Birgunj'),
('300960255', 'Narayani Oil Center', 'Inaruwa'),
('305346894', 'Nepal Fuel Center Pvt Ltd', 'Budiganga'),
('601235609', 'New Barsha Motor Parts', 'Birgunj'),
('301971119', 'Omshree Oil Stores', 'Kadamaha'),
('300661590', 'Pashupati Auto Mobiles Center Pvt Ltd', 'Birgunj'),
('605190386', 'Pathibhara Petrol Pump', 'Duhabi'),
('609188495', 'Raghunath Ray Radhakrishna', 'Biratnagar'),
('300257670', 'Rahul & Nisam Fuel Center Pvt Ltd', 'Budiganga'),
('300658299', 'Renuka Oil Stores', 'Birgunj'),
('303114765', 'Sagarmatha Fuel Center', 'Dhangadhimai'),
('302155754', 'Sathi Fuel Centre Pvt Ltd', 'Itahari'),
('302853492', 'Shivam Oil Center', 'Budiganga'),
('303927983', 'Shree Durga Oils', 'Birgunj'),
('300096509', 'Sunsari Oil Suppliers', 'Inaruwa'),
('300070859', 'The Bijaya Auto Service Center', 'Biratnagar'),
('302634882', 'Utakrista Trade Concern', 'Birgunj'),
('300073250', 'Worldlink Communication Ltd', 'Jawalkhel');
