SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";

CREATE TABLE IF NOT EXISTS `customers` (
  `id` int(10) UNSIGNED NOT NULL COMMENT 'Customer ID',
  `name` varchar(255) COLLATE utf8_unicode_ci NOT NULL COMMENT 'Name of the item.',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

INSERT INTO `customers` (`id`, `name`) VALUES
(1, 'Pavel'),
(2, 'Petr');

CREATE TABLE IF NOT EXISTS `favorite_items` (
  `customer_id` int(10) UNSIGNED NOT NULL COMMENT 'FK to the customers table.',
  `item_id` int(10) UNSIGNED NOT NULL COMMENT 'FK to the items table.',
  UNIQUE KEY `favorite_item` (`customer_id`,`item_id`),
  KEY `favorite_items_fk` (`item_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

INSERT INTO `favorite_items` (`customer_id`, `item_id`) VALUES
(1, 1),
(1, 2),
(2, 3),
(2, 4);

CREATE TABLE IF NOT EXISTS `items` (
  `id` int(10) UNSIGNED NOT NULL COMMENT 'ID of the item that can appear on the list',
  `name` varchar(255) COLLATE utf8_unicode_ci NOT NULL COMMENT 'Name of the item.',
  PRIMARY KEY (`id`),
  UNIQUE KEY `name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci COMMENT='A lookup table of all items that an be added to a shopping list.';

INSERT INTO `items` (`id`, `name`) VALUES
(4, 'beer'),
(1, 'bread'),
(2, 'butter'),
(5, 'chocolate'),
(9, 'instant soup'),
(3, 'milk'),
(6, 'pork chops'),
(8, 'potatoes'),
(7, 'tomatoes');

CREATE TABLE IF NOT EXISTS `list` (
  `id` int(10) UNSIGNED NOT NULL COMMENT 'ID of the list item.',
  `item_id` int(10) UNSIGNED NOT NULL COMMENT 'FK to the items table.',
  `amount` int(11) NOT NULL COMMENT 'Required amount (how much we need of this particular item).',
  `position` int(11) NOT NULL COMMENT 'Shopping list order.',
  PRIMARY KEY (`id`),
  UNIQUE KEY `item_id` (`item_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

INSERT INTO `list` (`id`, `item_id`, `amount`, `position`) VALUES
(1, 1, 3, 1),
(2, 2, 4, 2),
(3, 3, 1, 3),
(4, 4, 5, 4);


ALTER TABLE `favorite_items`
  ADD CONSTRAINT `favorite_customers_fk` FOREIGN KEY (`customer_id`) REFERENCES `customers` (`id`),
  ADD CONSTRAINT `favorite_items_fk` FOREIGN KEY (`item_id`) REFERENCES `items` (`id`);

ALTER TABLE `list`
  ADD CONSTRAINT `items_fk` FOREIGN KEY (`item_id`) REFERENCES `items` (`id`);
